using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComparingStringsWithoutStringComparisonAnalyzer : DiagnosticAnalyzer
{
    internal enum CapitalizationFunction
    {
        None = 0,
        Ordinal = 1,
        Invariant = 2
    }

    private record CapitalizationContext(IMethodSymbol[] MethodSymbols, CapitalizationFunction CapitalizationFunction, string Function);

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ComparingStringsWithoutStringComparison,
        "A string is being compared through allocating a new string, e.g. using ToLower() or ToUpperInvariant(). Use a case-insensitive comparison instead which does not allocate.",
        "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS049-ComparingStringsWithoutStringComparison.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var stringSymbol = compilationContext.Compilation.GetSpecialType(SpecialType.System_String);
            var toLowerSymbols = stringSymbol.GetMembers("ToLower").OfType<IMethodSymbol>().ToArray();
            var toUpperSymbols = stringSymbol.GetMembers("ToUpper").OfType<IMethodSymbol>().ToArray();
            var toLowerInvariantSymbols = stringSymbol.GetMembers("ToLowerInvariant").OfType<IMethodSymbol>().ToArray();
            var toUpperInvariantSymbols = stringSymbol.GetMembers("ToUpperInvariant").OfType<IMethodSymbol>().ToArray();

            var capitalizationContexts = ImmutableArray.Create(
                new CapitalizationContext(toLowerSymbols, CapitalizationFunction.Ordinal, "ToLower"),
                new CapitalizationContext(toUpperSymbols, CapitalizationFunction.Ordinal, "ToUpper"),
                new CapitalizationContext(toLowerInvariantSymbols, CapitalizationFunction.Invariant, "ToLowerInvariant"),
                new CapitalizationContext(toUpperInvariantSymbols, CapitalizationFunction.Invariant, "ToUpperInvariant")
            );

            compilationContext.RegisterOperationAction(context => Analyze(context, capitalizationContexts), OperationKind.Binary, OperationKind.IsPattern);
        });
    }

    private static void Analyze(OperationAnalysisContext context, ImmutableArray<CapitalizationContext> capitalizationContexts)
    {
        var operands = context.Operation switch
        {
            IBinaryOperation binaryOperation when binaryOperation.OperatorKind is BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals
                => new[] { binaryOperation.LeftOperand, binaryOperation.RightOperand },
            IIsPatternOperation isPatternOperation when isPatternOperation.Pattern is IConstantPatternOperation or INegatedPatternOperation { Pattern: IConstantPatternOperation }
                => new[] { isPatternOperation.Value },
            _ => Array.Empty<IOperation>()
        };

        var foundCapitalizations = operands.Select(o => StringCapitalizationFunction(o, capitalizationContexts)).ToArray();
        if (foundCapitalizations.GroupBy(x => x.Instance, SymbolEqualityComparer.Default).Count() != operands.Length)
        {
            // referencing the same symbol in operands
            return;
        }

        foreach (var capitalization in foundCapitalizations)
        {
            if (capitalization.CapitalizationContext is { CapitalizationFunction: CapitalizationFunction.Ordinal or CapitalizationFunction.Invariant })
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("comparison", capitalization.CapitalizationContext.CapitalizationFunction == CapitalizationFunction.Ordinal ? "ordinal" : "invariant");
                properties.Add("function", capitalization.CapitalizationContext.Function);
                context.ReportDiagnostic(Diagnostic.Create(Rule, capitalization.Operand.Syntax.GetLocation(), properties.ToImmutable()));
                return;
            }
        }
    }

    private static (CapitalizationContext? CapitalizationContext, ISymbol? Instance, IOperation Operand) StringCapitalizationFunction(IOperation operand, ImmutableArray<CapitalizationContext> capitalizationContexts)
    {
        static (IInvocationOperation? Operation, ISymbol? Instance) getInvocation(IOperation op)
        {
            return op switch
            {
                IInvocationOperation inv => (inv, GetReferencedInstance(inv.Instance)),
                IConditionalAccessOperation { WhenNotNull: IConditionalAccessOperation or IInvocationOperation } cond => getInvocation(cond.WhenNotNull),
                _ => (default, GetReferencedInstance(op))
            };
        }

        var (operation, instance) = getInvocation(operand);

        if (operation is { Arguments.Length: 0 })
        {
            foreach (var capitalizationContext in capitalizationContexts)
            {
                if (capitalizationContext.MethodSymbols.Any(s => s.Equals(operation.TargetMethod, SymbolEqualityComparer.Default)))
                {
                    return (capitalizationContext, instance, operand);
                }
            }
        }

        return (default, instance, operand);
    }

    private static ISymbol? GetReferencedInstance(IOperation? operation) => operation switch
    {
        ILocalReferenceOperation localRef => localRef.Local,
        IParameterReferenceOperation paramRef => paramRef.Parameter,
        IFieldReferenceOperation fieldRef => fieldRef.Field,
        IMemberReferenceOperation memberRef => memberRef.Member,
        IInstanceReferenceOperation instanceRef => instanceRef.Type,
        IInvocationOperation invocation => GetReferencedInstance(invocation.Instance),
        IConditionalAccessInstanceOperation cond => GetReferencedInstance(cond.Ancestors().OfType<IConditionalAccessOperation>().LastOrDefault()),
        IConditionalAccessOperation conditional => GetReferencedInstance(conditional.Operation),
        _ => default
    };
}