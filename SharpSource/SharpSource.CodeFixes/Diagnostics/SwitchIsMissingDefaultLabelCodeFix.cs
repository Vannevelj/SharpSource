using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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

[ExportCodeFixProvider(DiagnosticId.SwitchIsMissingDefaultLabel + "CF", LanguageNames.CSharp), Shared]
public class SwitchIsMissingDefaultLabelCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(SwitchIsMissingDefaultLabelAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var switchToken = root?.FindNode(diagnosticSpan);
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

    private Task<Document> AddDefaultCaseAsync(Document document, CompilationUnitSyntax root, SwitchStatementSyntax switchBlock)
    {
        var argumentException =
            SyntaxFactory.ThrowStatement(SyntaxFactory.ParseExpression($"new ArgumentException(\"Unsupported value\")"))
                         .WithAdditionalAnnotations(Formatter.Annotation);
        var statements = SyntaxFactory.List(new List<StatementSyntax> { argumentException });
        var defaultCase = SyntaxFactory.SwitchSection(SyntaxFactory.List<SwitchLabelSyntax>(new[] { SyntaxFactory.DefaultSwitchLabel() }), statements);

        var newNode = switchBlock.AddSections(defaultCase.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation));
        var newRoot = root.ReplaceNode(switchBlock, newNode);

        newRoot = newRoot.AddUsingStatementIfMissing("System");

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}