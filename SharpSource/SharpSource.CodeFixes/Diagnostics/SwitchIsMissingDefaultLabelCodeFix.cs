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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SwitchIsMissingDefaultLabelCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SwitchIsMissingDefaultLabelAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var switchToken = root.FindNode(diagnosticSpan);
        var statement = switchToken?.AncestorsAndSelf().FirstOfKind<SwitchStatementSyntax>(SyntaxKind.SwitchStatement);
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
        var argumentException = ThrowStatement(ParseExpression($"new ArgumentException(\"Unsupported value\")")).WithAdditionalAnnotations(Formatter.Annotation);
        var statements = List(new List<StatementSyntax> { argumentException });
        var defaultCase = SwitchSection(List<SwitchLabelSyntax>(new[] { DefaultSwitchLabel() }), statements);

        var newNode = switchBlock.AddSections(defaultCase.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation, Simplifier.AddImportsAnnotation, SymbolAnnotation.Create("System.ArgumentException")));
        var newRoot = root.ReplaceNode(switchBlock, newNode);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}