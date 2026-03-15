using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SwitchDoesNotHandleAllEnumOptionsAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.SwitchDoesNotHandleAllEnumOptions,
        "Add cases for missing enum member.",
        "Missing enum member in switched cases.",
        Categories.Correctness,
        DiagnosticSeverity.Info,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS018-SwitchDoesNotHandleAllEnumOptions.md");

    public static DiagnosticDescriptor RuleWhenDefaultIsPresent => new(
        DiagnosticId.SwitchDoesNotHandleAllEnumOptions,
        "Add cases for missing enum member.",
        "Missing enum member in switched cases.",
        Categories.Correctness,
        DiagnosticSeverity.Info,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS018-SwitchDoesNotHandleAllEnumOptions.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleWhenDefaultIsPresent);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationAction(AnalyzeSwitchOperation, OperationKind.Switch, OperationKind.SwitchExpression);
    }

    private static void AnalyzeSwitchOperation(OperationAnalysisContext context)
    {
        var (switchValue, caseData) = context.Operation switch
        {
            ISwitchOperation switchOperation =>
                (
                    switchOperation.Value,
                    switchOperation.Cases.SelectMany(@case => @case.Clauses).Select(GetCaseData)
                ),
            ISwitchExpressionOperation switchExpression =>
                (
                    switchExpression.Value,
                    switchExpression.Arms.Select(arm => GetCaseData(arm.Pattern))
                ),
            _ => default
        };

        if (switchValue?.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
        {
            return;
        }

        var hasDefaultClause = false;

        var labelSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var (labelSymbol, isDefault, isSupported) in caseData)
        {
            if (!isSupported)
            {
                return;
            }

            if (isDefault)
            {
                hasDefaultClause = true;
                continue;
            }

            if (labelSymbol is not null)
            {
                labelSymbols.Add(labelSymbol);
            }
        }

        foreach (var member in enumType.GetMembers())
        {
            if (member.Name == WellKnownMemberNames.InstanceConstructorName)
            {
                continue;
            }

            if (!labelSymbols.Contains(member))
            {
                context.ReportDiagnostic(Diagnostic.Create(hasDefaultClause ? RuleWhenDefaultIsPresent : Rule, switchValue.Syntax.GetLocation()));
                return;
            }
        }
    }

    private static (ISymbol? LabelSymbol, bool IsDefault, bool IsSupported) GetCaseData(ICaseClauseOperation caseClauseOperation)
        => caseClauseOperation switch
        {
            ISingleValueCaseClauseOperation { Value: IFieldReferenceOperation enumReference } => (enumReference.Field, false, true),
            _ when caseClauseOperation.CaseKind == CaseKind.Default => (null, true, true),
            _ => (null, false, false),
        };

    private static (ISymbol? LabelSymbol, bool IsDefault, bool IsSupported) GetCaseData(IPatternOperation patternOperation)
        => patternOperation switch
        {
            IConstantPatternOperation { Value: IFieldReferenceOperation enumReference } => (enumReference.Field, false, true),
            IDiscardPatternOperation => (null, true, true),
            _ => (null, false, false),
        };
}