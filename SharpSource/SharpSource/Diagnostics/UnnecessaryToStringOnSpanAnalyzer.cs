using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnnecessaryToStringOnSpanAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.UnnecessaryToStringOnSpan,
        "Unnecessary ToString() call on Span<char> or ReadOnlySpan<char>",
        "Unnecessary ToString() call, an overload that accepts {0} is available",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS064-UnnecessaryToStringOnSpan.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var spanCharSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Span`1")?.Construct(compilationContext.Compilation.GetSpecialType(SpecialType.System_Char));
            var readOnlySpanCharSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1")?.Construct(compilationContext.Compilation.GetSpecialType(SpecialType.System_Char));

            if (spanCharSymbol is null && readOnlySpanCharSymbol is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(context => Analyze(context, spanCharSymbol, readOnlySpanCharSymbol), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol? spanCharSymbol, INamedTypeSymbol? readOnlySpanCharSymbol)
    {
        var invocation = (IInvocationOperation)context.Operation;

        // Check if this is a ToString() call
        if (invocation.TargetMethod.Name != "ToString" || invocation.TargetMethod.Parameters.Length != 0)
        {
            return;
        }

        // Check if the receiver is a Span<char> or ReadOnlySpan<char>
        var receiverType = invocation.Instance?.Type;
        if (receiverType is null)
        {
            return;
        }

        var isSpanChar = spanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(receiverType, spanCharSymbol);
        var isReadOnlySpanChar = readOnlySpanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(receiverType, readOnlySpanCharSymbol);

        if (!isSpanChar && !isReadOnlySpanChar)
        {
            return;
        }

        var spanTypeName = isSpanChar ? "Span<char>" : "ReadOnlySpan<char>";

        // Check if the result is used as an argument to a method call
        if (invocation.Parent is not IArgumentOperation argumentOperation)
        {
            return;
        }

        if (argumentOperation.Parent is not IInvocationOperation parentInvocation)
        {
            return;
        }

        // Find which parameter index corresponds to our argument
        var parameterIndex = -1;
        for (var i = 0; i < parentInvocation.Arguments.Length; i++)
        {
            if (parentInvocation.Arguments[i] == argumentOperation)
            {
                parameterIndex = i;
                break;
            }
        }

        if (parameterIndex < 0)
        {
            return;
        }

        var parentMethod = parentInvocation.TargetMethod;
        var containingType = parentMethod.ContainingType;

        // Look for overloads that would accept a Span<char> or ReadOnlySpan<char> instead
        var overloads = containingType.GetMembers(parentMethod.Name).OfType<IMethodSymbol>();

        foreach (var overload in overloads)
        {
            if (SymbolEqualityComparer.Default.Equals(overload, parentMethod))
            {
                continue;
            }

            if (IsCompatibleOverload(parentInvocation, overload, parameterIndex, spanCharSymbol, readOnlySpanCharSymbol))
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("parameterIndex", parameterIndex.ToString());
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), spanTypeName));
                return;
            }
        }
    }

    private static bool IsCompatibleOverload(IInvocationOperation currentInvocation, IMethodSymbol candidateOverload, int changedParameterIndex, INamedTypeSymbol? spanCharSymbol, INamedTypeSymbol? readOnlySpanCharSymbol)
    {
        var currentMethod = currentInvocation.TargetMethod;

        // Quick check: the candidate must have at least as many parameters as the index we're changing
        if (candidateOverload.Parameters.Length <= changedParameterIndex)
        {
            return false;
        }

        // The candidate must accept a Span<char> or ReadOnlySpan<char> at the changed parameter position
        var candidateParamType = candidateOverload.Parameters[changedParameterIndex].Type;
        var acceptsSpanChar = spanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(candidateParamType, spanCharSymbol);
        var acceptsReadOnlySpanChar = readOnlySpanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(candidateParamType, readOnlySpanCharSymbol);

        if (!acceptsSpanChar && !acceptsReadOnlySpanChar)
        {
            return false;
        }

        // Check if we can map all current arguments to the candidate overload
        var currentArgs = currentInvocation.Arguments;

        // Count required parameters in the candidate
        var requiredParamCount = candidateOverload.Parameters.Count(p => !p.IsOptional && !p.IsParams);

        // We need at least as many arguments (with one change) as required parameters
        // But we also need to not have too many
        if (currentArgs.Length < requiredParamCount)
        {
            return false;
        }

        // Check each current argument (except the changed one) to see if it's compatible
        for (var i = 0; i < currentArgs.Length; i++)
        {
            if (i == changedParameterIndex)
            {
                continue;
            }

            // If the candidate doesn't have this parameter position, check for params
            if (i >= candidateOverload.Parameters.Length)
            {
                var lastParam = candidateOverload.Parameters[candidateOverload.Parameters.Length - 1];
                if (!lastParam.IsParams)
                {
                    return false;
                }
                // Params array - check element type
                if (lastParam.Type is IArrayTypeSymbol arrayType)
                {
                    var argType = GetArgumentType(currentArgs[i]);
                    if (argType is null || !IsTypeCompatible(argType, arrayType.ElementType))
                    {
                        return false;
                    }
                }
                continue;
            }

            var candidateParam = candidateOverload.Parameters[i];
            var argType2 = GetArgumentType(currentArgs[i]);

            if (argType2 is null)
            {
                continue;
            }

            if (!IsTypeCompatible(argType2, candidateParam.Type))
            {
                return false;
            }
        }

        return true;
    }

    private static ITypeSymbol? GetArgumentType(IArgumentOperation argument)
    {
        var value = argument.Value;
        if (value is IConversionOperation conversion)
        {
            return conversion.Operand.Type;
        }

        return value.Type;
    }

    private static bool IsTypeCompatible(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
        {
            return true;
        }

        // Check for implicit conversions
        if (sourceType is INamedTypeSymbol sourceNamed && targetType is INamedTypeSymbol targetNamed)
        {
            // Check inheritance
            var current = sourceNamed;
            while (current is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, targetNamed))
                {
                    return true;
                }
                current = current.BaseType;
            }

            // Check interfaces
            foreach (var iface in sourceNamed.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface, targetNamed))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
