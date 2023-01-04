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

        foreach (var operand in operands)
        {
            var capitalizationContext = StringCapitalizationFunction(operand, capitalizationContexts);
            if (capitalizationContext is { CapitalizationFunction: CapitalizationFunction.Ordinal or CapitalizationFunction.Invariant })
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("comparison", capitalizationContext.CapitalizationFunction == CapitalizationFunction.Ordinal ? "ordinal" : "invariant");
                properties.Add("function", capitalizationContext.Function);
                context.ReportDiagnostic(Diagnostic.Create(Rule, operand.Syntax.GetLocation(), properties.ToImmutable()));
                return;
            }
        }
    }

    private static CapitalizationContext? StringCapitalizationFunction(IOperation operand, ImmutableArray<CapitalizationContext> capitalizationContexts)
    {
        CapitalizationContext? getContext(IInvocationOperation invocation)
        {
            if (invocation is { Arguments.Length: 0 })
            {
                foreach (var capitalizationContext in capitalizationContexts)
                {
                    if (capitalizationContext.MethodSymbols.Any(s => s.Equals(invocation.TargetMethod, SymbolEqualityComparer.Default)))
                    {
                        return capitalizationContext;
                    }
                }
            }

            return default;
        }

        return operand switch
        {
            IInvocationOperation inv => getContext(inv),
            IConditionalAccessOperation { WhenNotNull: IInvocationOperation { Arguments.Length: 0 } nestedInv } => getContext(nestedInv),
            _ => default
        };
    }
}