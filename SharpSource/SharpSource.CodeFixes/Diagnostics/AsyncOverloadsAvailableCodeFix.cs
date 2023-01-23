using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AsyncOverloadsAvailableCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AsyncOverloadsAvailableAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var invocation = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as InvocationExpressionSyntax;
        if (invocation == default)
        {
            return;
        }

        ExpressionSyntax? newExpression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(GetAsyncIdentifier(memberAccess.Name)),
            IdentifierNameSyntax identifierName => GetAsyncIdentifier(identifierName),
            GenericNameSyntax genericName => genericName.WithIdentifier(GetAsyncIdentifier(genericName).Identifier),
            MemberBindingExpressionSyntax memberBinding => memberBinding.WithName(GetAsyncIdentifier(memberBinding.Name)),
            _ => default
        };

        if (newExpression == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Use Async overload",
                x => UseAsyncOverload(context.Document, invocation, newExpression, root, diagnostic),
                AsyncOverloadsAvailableAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseAsyncOverload(Document document, InvocationExpressionSyntax invocation, ExpressionSyntax newExpression, SyntaxNode root, Diagnostic diagnostic)
    {
        var newInvocation = invocation.WithExpression(newExpression);

        var shouldAddCancellationToken = diagnostic.Properties["shouldAddCancellationToken"] == "true";
        var cancellationTokenName = diagnostic.Properties["cancellationTokenName"];
        if (shouldAddCancellationToken && cancellationTokenName != default)
        {
            var cancellationTokenIsOptional = diagnostic.Properties["cancellationTokenIsOptional"] == "true";
            var cancellationToken = cancellationTokenIsOptional ? SyntaxFactory.ParseExpression($"{cancellationTokenName} ?? CancellationToken.None") : SyntaxFactory.IdentifierName(cancellationTokenName);
            var newArguments = newInvocation.ArgumentList.AddArguments(SyntaxFactory.Argument(cancellationToken));
            newInvocation = newInvocation.WithArgumentList(newArguments);
        }

        ExpressionSyntax newExpressionToAwait = newInvocation;
        if (invocation.Parent is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            newExpressionToAwait = conditionalAccess.WithWhenNotNull(newInvocation);
        }

        ExpressionSyntax awaitExpression = SyntaxFactory.AwaitExpression(newExpressionToAwait.WithoutTrivia()).WithAdditionalAnnotations(Formatter.Annotation);

        // If we're accessing the result of the method call, i.e. `DoThing().Property` then we need to wrap the `await` expression with parentheses
        if (invocation.Parent is MemberAccessExpressionSyntax)
        {
            awaitExpression = SyntaxFactory.ParenthesizedExpression(awaitExpression);
        }

        var newRoot = root.ReplaceNode(invocation.Parent is ConditionalAccessExpressionSyntax ? invocation.Parent : invocation, awaitExpression.WithTriviaFrom(invocation));
        var newDocument = document.WithSyntaxRoot(newRoot);

        return Task.FromResult(newDocument);
    }

    private static IdentifierNameSyntax GetAsyncIdentifier(SimpleNameSyntax nameSyntax) => SyntaxFactory.IdentifierName($"{nameSyntax.Identifier.ValueText}Async");
}