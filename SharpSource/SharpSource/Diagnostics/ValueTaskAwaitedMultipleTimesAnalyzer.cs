using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ValueTaskAwaitedMultipleTimesAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ValueTaskAwaitedMultipleTimes,
        "A ValueTask was awaited multiple times",
        "A ValueTask can only be awaited once",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS063-ValueTaskAwaitedMultipleTimes.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var valueTaskSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
            var valueTaskGenericSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

            if (valueTaskSymbol is null && valueTaskGenericSymbol is null)
            {
                return;
            }

            compilationContext.RegisterOperationBlockAction(context => AnalyzeOperationBlock(context, valueTaskSymbol, valueTaskGenericSymbol));
        });
    }

    private static void AnalyzeOperationBlock(OperationBlockAnalysisContext context, INamedTypeSymbol? valueTaskSymbol, INamedTypeSymbol? valueTaskGenericSymbol)
    {
        var awaitedLocals = new Dictionary<ILocalSymbol, Location>(SymbolEqualityComparer.Default);
        var awaitedParameters = new Dictionary<IParameterSymbol, Location>(SymbolEqualityComparer.Default);

        foreach (var operation in context.OperationBlocks)
        {
            AnalyzeOperation(operation, valueTaskSymbol, valueTaskGenericSymbol, awaitedLocals, awaitedParameters, context);
        }
    }

    private static void AnalyzeOperation(
        IOperation operation,
        INamedTypeSymbol? valueTaskSymbol,
        INamedTypeSymbol? valueTaskGenericSymbol,
        Dictionary<ILocalSymbol, Location> awaitedLocals,
        Dictionary<IParameterSymbol, Location> awaitedParameters,
        OperationBlockAnalysisContext context)
    {
        if (operation is IAwaitOperation awaitOperation)
        {
            var awaitedValue = awaitOperation.Operation;

            // Unwrap conversion operations (e.g., implicit conversions)
            while (awaitedValue is IConversionOperation conversion)
            {
                awaitedValue = conversion.Operand;
            }

            // Handle ConfigureAwait calls (e.g., task.ConfigureAwait(false))
            if (awaitedValue is IInvocationOperation invocation && invocation.TargetMethod.Name == "ConfigureAwait")
            {
                awaitedValue = invocation.Instance;
                while (awaitedValue is IConversionOperation conv)
                {
                    awaitedValue = conv.Operand;
                }
            }

            if (awaitedValue is ILocalReferenceOperation localRef)
            {
                var localSymbol = localRef.Local;
                if (!IsValueTaskType(localSymbol.Type, valueTaskSymbol, valueTaskGenericSymbol))
                {
                    // Not a ValueTask, ignore
                }
                else if (awaitedLocals.TryGetValue(localSymbol, out var previousLocation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, awaitOperation.Syntax.GetLocation()));
                }
                else
                {
                    awaitedLocals[localSymbol] = awaitOperation.Syntax.GetLocation();
                }
            }
            else if (awaitedValue is IParameterReferenceOperation paramRef)
            {
                var paramSymbol = paramRef.Parameter;
                if (!IsValueTaskType(paramSymbol.Type, valueTaskSymbol, valueTaskGenericSymbol))
                {
                    // Not a ValueTask, ignore
                }
                else if (awaitedParameters.TryGetValue(paramSymbol, out var previousLocation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, awaitOperation.Syntax.GetLocation()));
                }
                else
                {
                    awaitedParameters[paramSymbol] = awaitOperation.Syntax.GetLocation();
                }
            }
        }

        foreach (var child in operation.ChildOperations)
        {
            AnalyzeOperation(child, valueTaskSymbol, valueTaskGenericSymbol, awaitedLocals, awaitedParameters, context);
        }
    }

    private static bool IsValueTaskType(ITypeSymbol? type, INamedTypeSymbol? valueTaskSymbol, INamedTypeSymbol? valueTaskGenericSymbol)
    {
        if (type is null)
        {
            return false;
        }

        if (valueTaskSymbol is not null && type.Equals(valueTaskSymbol, SymbolEqualityComparer.Default))
        {
            return true;
        }

        if (valueTaskGenericSymbol is not null && type.OriginalDefinition.Equals(valueTaskGenericSymbol, SymbolEqualityComparer.Default))
        {
            return true;
        }

        return false;
    }
}