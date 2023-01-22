using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnnecessaryEnumerableMaterializationAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.UnnecessaryEnumerableMaterialization,
        "An IEnumerable was materialized before a deferred execution call",
        "{0} is unnecessarily materializing the IEnumerable and can be omitted",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS041-UnnecessaryEnumerableMaterialization.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var enumerableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if (enumerableSymbol is null)
            {
                return;
            }

            // An array is used instead of a hash set since the number of elements is small. HashSet is likely to be slower for searching in this case.
            var materializingSymbols = enumerableSymbol.GetAllMembers("ToList", "ToArray", "ToHashSet").ToArray();
            var deferredExecutionSymbols = enumerableSymbol.GetAllMembers(
                "Select", "SelectMany", "Take", "Skip", "TakeWhile", "SkipWhile", "SkipLast", "Where", "GroupBy", "GroupJoin", "OrderBy", "OrderByDescending", "Union",
                       "UnionBy", "Zip", "Reverse", "Join", "OfType", "Intersect", "IntersectBy", "Except", "ExceptBy", "Distinct", "DistinctBy", "DefaultIfEmpty", "Concat", "Cast").ToImmutableHashSet(SymbolEqualityComparer.Default);

            compilationContext.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;
                var precedingInvocation = invocation.GetPrecedingInvocation()?.TargetMethod.OriginalDefinition;
                if (precedingInvocation is null)
                {
                    return;
                }

                var precedingSymbol = materializingSymbols.FirstOrDefault(s => s.Equals(precedingInvocation, SymbolEqualityComparer.Default));
                if (precedingSymbol is null)
                {
                    return;
                }

                var subsequentSymbol = deferredExecutionSymbols.FirstOrDefault(s => s.Equals(precedingInvocation, SymbolEqualityComparer.Default)) ?? materializingSymbols.FirstOrDefault(s => s.Equals(precedingInvocation, SymbolEqualityComparer.Default));
                if (subsequentSymbol is null)
                {
                    return;
                }

                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("operation", precedingSymbol.Name);
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), $"{precedingSymbol.Name}"));
            }, OperationKind.Invocation);
        });
    }
}