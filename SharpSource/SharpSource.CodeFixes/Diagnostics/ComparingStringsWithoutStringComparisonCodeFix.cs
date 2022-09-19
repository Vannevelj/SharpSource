using System;
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

[ExportCodeFixProvider(DiagnosticId.ComparingStringsWithoutStringComparison + "CF", LanguageNames.CSharp), Shared]
public class ComparingStringsWithoutStringComparisonCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.ComparingStringsWithoutStringComparison);

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

        //var stackallocCreation = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<StackAllocArrayCreationExpressionSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create("Use StringComparison.OrdinalIgnoreCase",
                x => UseOrdinalIgnoreCase(context.Document, root),
                DiagnosticId.ComparingStringsWithoutStringComparison),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create("Use StringComparison.InvariantCultureIgnoreCase",
                x => UseInvariantCultureIgnoreCase(context.Document, root),
                DiagnosticId.ComparingStringsWithoutStringComparison),
            diagnostic);
    }

    private Task<Document> UseOrdinalIgnoreCase(Document document, SyntaxNode root) => Task.FromResult(document);
    private Task<Document> UseInvariantCultureIgnoreCase(Document document, SyntaxNode root) => Task.FromResult(document);
}