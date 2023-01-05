using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
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

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SwitchIsMissingDefaultLabelCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(SwitchIsMissingDefaultLabelAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var switchToken = root.FindNode(diagnosticSpan);
        var statement = switchToken?.DescendantNodesAndSelf().FirstOfKind<SwitchStatementSyntax>(SyntaxKind.SwitchStatement);
        if (root is not CompilationUnitSyntax compilation || statement == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Add default label",
                x => AddDefaultCaseAsync(context.Document, compilation, statement),
                SwitchIsMissingDefaultLabelAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> AddDefaultCaseAsync(Document document, CompilationUnitSyntax root, SwitchStatementSyntax switchBlock)
    {
        var argumentException =
            SyntaxFactory.ThrowStatement(SyntaxFactory.ParseExpression($"new ArgumentException(\"Unsupported value\")"))
                         .WithAdditionalAnnotations(Formatter.Annotation);
        var statements = SyntaxFactory.List(new List<StatementSyntax> { argumentException });
        var defaultCase = SyntaxFactory.SwitchSection(SyntaxFactory.List<SwitchLabelSyntax>(new[] { SyntaxFactory.DefaultSwitchLabel() }), statements);

        var newNode = switchBlock.AddSections(defaultCase.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation, Simplifier.AddImportsAnnotation));
        var newRoot = root.ReplaceNode(switchBlock, newNode);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}