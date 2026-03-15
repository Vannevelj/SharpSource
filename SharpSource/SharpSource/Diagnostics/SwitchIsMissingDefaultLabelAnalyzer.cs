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
        DiagnosticSeverity.Info,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS019-SwitchIsMissingDefaultLabel.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationAction(AnalyzeSwitchOperation, OperationKind.Switch, OperationKind.SwitchExpression);
    }

    private static void AnalyzeSwitchOperation(OperationAnalysisContext context)
    {
        var (value, hasDefault) = context.Operation switch
        {
            ISwitchOperation surroundingSwitch =>
                (
                    surroundingSwitch.Value,
                    surroundingSwitch.Cases.SelectMany(c => c.Clauses).OfType<IDefaultCaseClauseOperation>().Any()
                ),
            ISwitchExpressionOperation switchExpression =>
                (
                    switchExpression.Value,
                    switchExpression.Arms.Any(arm => arm.Pattern is IDiscardPatternOperation)
                ),
            _ => default
        };

        if (value is not null && !hasDefault)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, value.Syntax.GetLocation()));
        }
    }
}