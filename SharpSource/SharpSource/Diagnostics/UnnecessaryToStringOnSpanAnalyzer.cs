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
        var currentParamCount = currentMethod.Parameters.Length;

        // The candidate must have at least as many parameters as the current method
        if (candidateOverload.Parameters.Length < currentParamCount)
        {
            return false;
        }

        // Any extra parameters in the candidate must be optional
        for (var i = currentParamCount; i < candidateOverload.Parameters.Length; i++)
        {
            if (!candidateOverload.Parameters[i].IsOptional)
            {
                return false;
            }
        }

        // The candidate must accept a Span<char> or ReadOnlySpan<char> at the changed parameter position
        var candidateParamType = candidateOverload.Parameters[changedParameterIndex].Type;
        var acceptsSpanChar = spanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(candidateParamType, spanCharSymbol);
        var acceptsReadOnlySpanChar = readOnlySpanCharSymbol is not null && SymbolEqualityComparer.Default.Equals(candidateParamType, readOnlySpanCharSymbol);

        if (!acceptsSpanChar && !acceptsReadOnlySpanChar)
        {
            return false;
        }

        // Check that all other parameters have matching types
        for (var i = 0; i < currentParamCount; i++)
        {
            if (i == changedParameterIndex)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(currentMethod.Parameters[i].Type, candidateOverload.Parameters[i].Type))
            {
                return false;
            }
        }

        return true;
    }
}