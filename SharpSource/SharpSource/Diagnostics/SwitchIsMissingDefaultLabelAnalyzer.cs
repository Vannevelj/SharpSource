using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SwitchIsMissingDefaultLabelAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.SwitchIsMissingDefaultLabel,
        "Switch should have default label.",
        "Switch should have default label.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS019-SwitchIsMissingDefaultLabel.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SwitchStatement);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var switchBlock = (SwitchStatementSyntax)context.Node;

        var hasDefaultLabel = false;
        foreach (var section in switchBlock.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label.IsKind(SyntaxKind.DefaultSwitchLabel))
                {
                    hasDefaultLabel = true;
                }
            }
        }

        if (!hasDefaultLabel)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, switchBlock.SwitchKeyword.GetLocation()));
        }
    }
}