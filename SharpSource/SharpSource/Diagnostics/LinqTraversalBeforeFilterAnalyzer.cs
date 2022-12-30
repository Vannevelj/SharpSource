using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LinqTraversalBeforeFilterAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> TraversalOperations = new(){
        "OrderBy", "OrderByDescending", "Chunk", "Reverse", "Take", "TakeLast", "TakeWhile"
    };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LinqTraversalBeforeFilter,
        "An IEnumerable extension method was used to traverse the collection and is subsequently filtered using Where()." +
        "If the Where() filter is executed first, the traversal will have to iterate over fewer items which will result in better performance.",
        "Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering if performed first?",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS047-LinqTraversalBeforeFilter.md");

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

            var whereSymbols = enumerableSymbol.GetMembers("Where").OfType<IMethodSymbol>().ToArray();
            var traversalOperationSymbols = TraversalOperations.SelectMany(op => enumerableSymbol.GetMembers(op).OfType<IMethodSymbol>()).ToArray();
            compilationContext.RegisterOperationAction(context => Analyze(context, traversalOperationSymbols, whereSymbols), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, IMethodSymbol[] traversalOperationSymbols, IMethodSymbol[] whereSymbols)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (!traversalOperationSymbols.Any(symbol => symbol.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var operation = context.Operation.Parent;
        while (operation != null)
        {
            if (operation is IInvocationOperation previousInvocation)
            {
                if (whereSymbols.Any(symbol => symbol.Equals(previousInvocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
                break;
            }
            operation = operation.Parent;
        }
    }
}