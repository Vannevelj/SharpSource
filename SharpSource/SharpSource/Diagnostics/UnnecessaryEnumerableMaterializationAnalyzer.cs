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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var enumerableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            var listSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            var iQueryableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Linq.IQueryable");

            // An array is used instead of a hash set since the number of elements is small. HashSet is likely to be slower for searching in this case.
            var materializingSymbols = enumerableSymbol?.GetAllMembers("ToList", "ToArray", "ToHashSet").ToArray();
            var deferredExecutionSymbols =
                enumerableSymbol?.GetAllMembers(
                "Select", "SelectMany", "Take", "Skip", "TakeWhile", "SkipWhile", "SkipLast", "Where", "GroupBy", "GroupJoin", "OrderBy", "OrderByDescending", "Union",
                       "UnionBy", "Zip", "Reverse", "Join", "OfType", "Intersect", "IntersectBy", "Except", "ExceptBy", "Distinct", "DistinctBy", "DefaultIfEmpty", "Concat", "Cast")
                .Concat(listSymbol?.GetAllMembers("Reverse"))
                .ToImmutableHashSet(SymbolEqualityComparer.Default);

            compilationContext.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;
                var precedingInvocation = invocation.GetPrecedingInvocation();
                if (precedingInvocation is null)
                {
                    return;
                }

                var precedingMethod = precedingInvocation.TargetMethod.OriginalDefinition;
                var subsequentInvocation = invocation.TargetMethod.OriginalDefinition;
                if (precedingMethod is null)
                {
                    return;
                }

                var precedingSymbol = materializingSymbols.FirstOrDefault(s => s.Equals(precedingMethod, SymbolEqualityComparer.Default));
                if (precedingSymbol is null)
                {
                    return;
                }

                var subsequentSymbol = deferredExecutionSymbols.FirstOrDefault(s => s.Equals(subsequentInvocation, SymbolEqualityComparer.Default)) ?? materializingSymbols.FirstOrDefault(s => s.Equals(subsequentInvocation, SymbolEqualityComparer.Default));
                if (subsequentSymbol is null)
                {
                    return;
                }

                if (iQueryableSymbol is not null)
                {
                    var objectBeingLinqed = precedingInvocation.GetTypeOfInstanceInInvocation();
                    if (objectBeingLinqed is not null && objectBeingLinqed.InheritsFrom(iQueryableSymbol))
                    {
                        return;
                    }
                }

                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("operation", precedingSymbol.Name);
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), $"{precedingSymbol.Name}"));
            }, OperationKind.Invocation);
        });
    }
}