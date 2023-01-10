using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MultipleOrderByCallsAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.MultipleOrderByCalls,
        "Successive OrderBy() calls will maintain only the last specified sort order. Use ThenBy() to combine them",
        "Successive OrderBy() calls will maintain only the last specified sort order",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS055-MultipleOrderByCalls.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var enumerableSymbol = context.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            var orderBySymbols = enumerableSymbol?.GetMembers("OrderBy").Concat(enumerableSymbol?.GetMembers("OrderByDescending")).OfType<IMethodSymbol>().ToArray();

            if (orderBySymbols is not null)
            {
                context.RegisterOperationAction(context => Analyze(context, orderBySymbols), OperationKind.Invocation);
            }            
        });
    }

    private static void Analyze(OperationAnalysisContext context, IMethodSymbol[] orderByMethods)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (!orderByMethods.Any(symbol => symbol.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var operation = invocation.Parent;
        while (operation != null)
        {
            if (operation is IInvocationOperation previousInvocation)
            {
                if (orderByMethods.Any(symbol => symbol.Equals(previousInvocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default)))
                {
                    var newName = previousInvocation.TargetMethod.OriginalDefinition.Name == "OrderBy" ? "ThenBy" : "ThenByDescending";
                    var properties = ImmutableDictionary<string, string?>.Empty.Add("NewName", newName);
                    context.ReportDiagnostic(Diagnostic.Create(Rule, previousInvocation.Syntax.GetLocation(), properties));
                }
                break;
            }
            operation = operation.Parent;
        }
    }
}