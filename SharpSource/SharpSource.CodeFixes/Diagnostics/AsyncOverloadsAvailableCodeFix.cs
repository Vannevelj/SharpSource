using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(DiagnosticId.AsyncOverloadsAvailable + "CF", LanguageNames.CSharp), Shared]
public class AsyncOverloadsAvailableCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AsyncOverloadsAvailableAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == default)
        {
            return;
        }
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var currentContextHasCancellationToken = diagnostic.Properties["currentContextHasCancellationToken"] == "true";
        var currentInvocationHasCancellationToken = diagnostic.Properties["currentInvocationHasCancellationToken"] == "true";
        var newInvocationAcceptsCancellationToken = diagnostic.Properties["newInvocationAcceptsCancellationToken"] == "true";
        var currentContextHasOptionalCancellationToken = diagnostic.Properties["currentContextHasOptionalCancellationToken"] == "true";

        var invocation = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();
        if (invocation == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Use Async overload",
                x => UseAsyncOverload(
                    context.Document, invocation, root,
                    currentContextHasCancellationToken, currentInvocationHasCancellationToken, newInvocationAcceptsCancellationToken, currentContextHasOptionalCancellationToken),
                AsyncOverloadsAvailableAnalyzer.Rule.Id),
            diagnostic);
    }

    private Task<Document> UseAsyncOverload(
        Document document, InvocationExpressionSyntax invocation, SyntaxNode root,
        bool currentContextHasCancellationToken, bool currentInvocationHasCancellationToken, bool newInvocationAcceptsCancellationToken, bool currentContextHasOptionalCancellationToken)
    {
        ExpressionSyntax? newExpression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(GetIdentifier(memberAccess.Name)),
            IdentifierNameSyntax identifierName => GetIdentifier(identifierName),
            GenericNameSyntax genericName => genericName.WithIdentifier(GetIdentifier(genericName).Identifier),
            _ => default
        };

        if (newExpression == default)
        {
            return Task.FromResult(document);
        }

        var newInvocation = invocation.WithExpression(newExpression);
        if (newInvocationAcceptsCancellationToken)
        {
            if (currentContextHasCancellationToken && !currentInvocationHasCancellationToken)
            {
                var cancellationToken = currentContextHasOptionalCancellationToken ? SyntaxFactory.ParseExpression("token ?? CancellationToken.None") : SyntaxFactory.IdentifierName("token");
                var newArguments = newInvocation.ArgumentList.AddArguments(SyntaxFactory.Argument(cancellationToken));
                newInvocation = newInvocation.WithArgumentList(newArguments);
            }
        }

        ExpressionSyntax awaitExpression = SyntaxFactory.AwaitExpression(newInvocation).WithAdditionalAnnotations(Formatter.Annotation);

        // If we're accessing the result of the method call, i.e. `DoThing().Property` then we need to wrap the `await` expression with parentheses
        if (invocation.Parent is MemberAccessExpressionSyntax)
        {
            awaitExpression = SyntaxFactory.ParenthesizedExpression(awaitExpression);
        }

        var newRoot = root.ReplaceNode(invocation, awaitExpression);
        var newDocument = document.WithSyntaxRoot(newRoot);

        return Task.FromResult(newDocument);
    }

    private IdentifierNameSyntax GetIdentifier(SimpleNameSyntax nameSyntax) => SyntaxFactory.IdentifierName($"{nameSyntax.Identifier.ValueText}Async");
}