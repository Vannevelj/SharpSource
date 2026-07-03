using System.Collections.Generic;
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
        if (context.Operation is ISwitchOperation switchOp)
        {
            var hasDefault = switchOp.Cases.SelectMany(c => c.Clauses).OfType<IDefaultCaseClauseOperation>().Any();
            if (!hasDefault && !IsExhaustiveBoolSwitch(switchOp) && !IsPatternMatchingSwitch(switchOp))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, switchOp.Value.Syntax.GetLocation()));
            }
        }
        else if (context.Operation is ISwitchExpressionOperation switchExpr)
        {
            var hasDiscard = switchExpr.Arms.Any(arm => arm.Pattern is IDiscardPatternOperation);
            if (!hasDiscard && !IsExhaustiveBoolSwitchExpression(switchExpr))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, switchExpr.Value.Syntax.GetLocation()));
            }
        }
    }

    // bool only has two values — if both are covered the switch is exhaustive
    private static bool IsExhaustiveBoolSwitch(ISwitchOperation switchOp)
    {
        if (switchOp.Value.Type?.SpecialType != SpecialType.System_Boolean)
        {
            return false;
        }

        var coveredValues = new HashSet<bool>(
            switchOp.Cases
                .SelectMany(c => c.Clauses)
                .OfType<ISingleValueCaseClauseOperation>()
                .Select(c => c.Value.ConstantValue)
                .Where(cv => cv.HasValue && cv.Value is bool)
                .Select(cv => (bool)cv.Value!));

        return coveredValues.Contains(true) && coveredValues.Contains(false);
    }

    // bool switch expression with both true and false arms is exhaustive
    private static bool IsExhaustiveBoolSwitchExpression(ISwitchExpressionOperation switchExpr)
    {
        if (switchExpr.Value.Type?.SpecialType != SpecialType.System_Boolean)
        {
            return false;
        }

        var coveredValues = new HashSet<bool>(
            switchExpr.Arms
                .Select(a => a.Pattern)
                .OfType<IConstantPatternOperation>()
                .Select(p => p.Value.ConstantValue)
                .Where(cv => cv.HasValue && cv.Value is bool)
                .Select(cv => (bool)cv.Value!));

        return coveredValues.Contains(true) && coveredValues.Contains(false);
    }

    // Pattern-matching switches on type hierarchies are intentionally partial — the hierarchy is open
    // and the developer handles only the cases they care about
    private static bool IsPatternMatchingSwitch(ISwitchOperation switchOp)
    {
        var nonDefaultClauses = switchOp.Cases
            .SelectMany(c => c.Clauses)
            .Where(c => c is not IDefaultCaseClauseOperation)
            .ToList();

        if (nonDefaultClauses.Count == 0)
        {
            return false;
        }

        if (!nonDefaultClauses.All(c => c is IPatternCaseClauseOperation))
        {
            return false;
        }

        // Require at least one non-constant pattern (type, property, relational, declaration, etc.)
        // to distinguish from plain constant matching that happens to use pattern syntax
        return nonDefaultClauses.Cast<IPatternCaseClauseOperation>()
            .Any(c => c.Pattern is not IConstantPatternOperation);
    }
}
