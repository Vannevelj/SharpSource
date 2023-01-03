using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class OnPropertyChangedWithoutNameOfOperatorCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(OnPropertyChangedWithoutNameOfOperatorAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];

        context.RegisterCodeFix(
            CodeAction.Create("Use nameof()",
                x => UseNameOfAsync(context.Document, root, diagnostic),
                OnPropertyChangedWithoutNameOfOperatorAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> UseNameOfAsync(Document document, SyntaxNode root, Diagnostic diagnostic)
    {
        var propertyName = diagnostic.Properties["parameterName"]!;
        var nodeToReplace = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var newRoot = root.ReplaceNode(nodeToReplace, SyntaxFactory.ParseExpression($"nameof({propertyName})"));
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}