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
        DiagnosticSeverity.Warning,
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterOperationAction(AnalyzeSwitchOperation, OperationKind.Switch);
    }

    private static void AnalyzeSwitchOperation(OperationAnalysisContext context)
    {
        var switchOperation = (ISwitchOperation)context.Operation;
        if (switchOperation.Value.Type is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
        {
            return;
        }

        var hasDefaultClause = false;

        var labelSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var caseOperation in switchOperation.Cases)
        {
            foreach (var caseClauseOperation in caseOperation.Clauses)
            {
                if (caseClauseOperation is ISingleValueCaseClauseOperation { Value: IFieldReferenceOperation enumReference })
                {
                    labelSymbols.Add(enumReference.Field);
                }
                else if (caseClauseOperation.CaseKind == CaseKind.Default)
                {
                    hasDefaultClause = true;
                }
                else
                {
                    return;
                }
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
                context.ReportDiagnostic(Diagnostic.Create(hasDefaultClause ? RuleWhenDefaultIsPresent : Rule, switchOperation.Value.Syntax.GetLocation()));
                return;
            }
        }
    }
}