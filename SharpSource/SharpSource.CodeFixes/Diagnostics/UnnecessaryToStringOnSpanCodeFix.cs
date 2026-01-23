using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UnnecessaryToStringOnSpanCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnnecessaryToStringOnSpanAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var invocation = root.FindNode(diagnosticSpan).DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Remove unnecessary ToString() call",
                x => RemoveToStringCall(context.Document, root, invocation), UnnecessaryToStringOnSpanAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> RemoveToStringCall(Document document, SyntaxNode root, InvocationExpressionSyntax toStringInvocation)
    {
        // The invocation is something like: span.ToString()
        // We want to replace it with just: span
        if (toStringInvocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var spanExpression = memberAccess.Expression;
            var newRoot = root.ReplaceNode(toStringInvocation, spanExpression.WithTriviaFrom(toStringInvocation));
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }

        return Task.FromResult(document);
    }
}
