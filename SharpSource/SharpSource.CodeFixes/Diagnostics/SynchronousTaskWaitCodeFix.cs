using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SynchronousTaskWaitCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SynchronousTaskWaitAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var synchronousWaitMethod = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as InvocationExpressionSyntax;
        var memberAccessExpression = synchronousWaitMethod?.DescendantNodesAndSelfOfType(SyntaxKind.SimpleMemberAccessExpression).FirstOrDefault() as MemberAccessExpressionSyntax;

        // If arguments are passed in then we don't want to offer a code fix as there is no straight way to include that functionality.
        if (diagnostic.Properties.TryGetValue("numberOfArguments", out var numberOfArguments) && numberOfArguments != "0")
        {
            return;
        }

        if (synchronousWaitMethod == default || memberAccessExpression == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Use await",
                x => UseAwait(context.Document, synchronousWaitMethod, memberAccessExpression, root),
                SynchronousTaskWaitAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseAwait(Document document, InvocationExpressionSyntax invocationExpression, MemberAccessExpressionSyntax memberAccessExpression, SyntaxNode root)
    {
        var leadingTrivia = memberAccessExpression.GetLeadingTrivia();
        var newExpression = AwaitExpression(memberAccessExpression.Expression.WithoutLeadingTrivia()).WithLeadingTrivia(leadingTrivia);

        var newRoot = root.ReplaceNode(invocationExpression, newExpression);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}