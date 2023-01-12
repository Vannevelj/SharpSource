using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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
        context.RegisterOperationAction(context =>
        {
            var cs = (ISwitchCaseOperation)context.Operation;
            var surroundingSwitch = cs.Ancestors().OfType<ISwitchOperation>().FirstOrDefault();
            if (surroundingSwitch == default)
            {
                return;
            }

            if (!surroundingSwitch.Cases.SelectMany(c => c.Clauses).OfType<IDefaultCaseClauseOperation>().Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, surroundingSwitch.Syntax.GetLocation()));
            }
        }, OperationKind.SwitchCase);
    }
}