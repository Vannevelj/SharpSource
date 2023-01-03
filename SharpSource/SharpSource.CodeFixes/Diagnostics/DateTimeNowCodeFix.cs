using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DateTimeNowCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DateTimeNowAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var statement = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        context.RegisterCodeFix(
            CodeAction.Create("Use DateTime.UtcNow",
                x => UseUtc(context.Document, root, statement), DateTimeNowAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> UseUtc(Document document, SyntaxNode root, SyntaxNode statement)
    {
        var newRoot = root.ReplaceNode(statement, SyntaxFactory.ParseExpression("System.DateTime.UtcNow").WithAdditionalAnnotations(Simplifier.Annotation));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}