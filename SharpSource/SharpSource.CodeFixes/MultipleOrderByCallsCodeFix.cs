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

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MultipleOrderByCallsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MultipleOrderByCallsAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var invocation = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as InvocationExpressionSyntax;
        var newName = diagnostic.Properties["NewName"];
        if (invocation == default || newName == default)
        {
            return;
        }

        ExpressionSyntax? newExpression = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(SyntaxFactory.IdentifierName(newName)),
            _ => default
        };

        if (newExpression == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Replace with ThenBy()",
                x => Replace(context.Document, invocation, newExpression, root),
                UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> Replace(Document document, InvocationExpressionSyntax invocation, ExpressionSyntax newExpression, SyntaxNode root)
    {
        var newRoot = root.ReplaceNode(invocation.Expression, newExpression);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}