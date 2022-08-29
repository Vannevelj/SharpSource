using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
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
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(CodeFixResources.AsyncOverloadsAvailableCodeFixTitle,
                x => UseAsyncOverload(context.Document, memberAccess, root, x),
                AsyncOverloadsAvailableAnalyzer.Rule.Id),
            diagnostic);
    }

    private Task<Document> UseAsyncOverload(Document document, InvocationExpressionSyntax invocation, SyntaxNode root, CancellationToken cancellationToken)
    {
        ExpressionSyntax newExpression;
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            newExpression = memberAccess.WithName(GetIdentifier(memberAccess.Name));
        }
        else if (invocation.Expression is IdentifierNameSyntax identifierName)
        {
            newExpression = GetIdentifier(identifierName);
        }
        else if (invocation.Expression is GenericNameSyntax genericName)
        {
            newExpression = genericName.WithIdentifier(GetIdentifier(genericName).Identifier);
        }
        else
        {
            return Task.FromResult(document);
        }

        var newInvocation = invocation.WithExpression(newExpression);
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