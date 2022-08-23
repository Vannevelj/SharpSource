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
    private static readonly string Message = "Switch should have default label.";
    private static readonly string Title = "Switch is missing a default label.";

    public static DiagnosticDescriptor Rule
        => new(DiagnosticId.SwitchIsMissingDefaultLabel, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

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