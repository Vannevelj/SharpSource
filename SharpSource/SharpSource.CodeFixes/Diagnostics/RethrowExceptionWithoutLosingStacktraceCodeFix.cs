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
public class RethrowExceptionWithoutLosingStacktraceCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(RethrowExceptionWithoutLosingStacktraceAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var throwStatement = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<ThrowStatementSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create("Remove rethrow",
                x => RemoveRethrowAsync(context.Document, root, throwStatement),
                RethrowExceptionWithoutLosingStacktraceAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> RemoveRethrowAsync(Document document, SyntaxNode root, ThrowStatementSyntax throwStatement)
    {
        var newStatement = SyntaxFactory.ThrowStatement();
        var newRoot = root.ReplaceNode(throwStatement, newStatement);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}