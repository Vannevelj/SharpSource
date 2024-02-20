using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PointlessCollectionToStringAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.PointlessCollectionToString,
        ".ToString() was called on a collection which results in impractical output. Considering using string.Join() to display the values instead.",
        ".ToString() was called on a collection which results in impractical output",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS053-PointlessCollectionToString.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (invocation is not { TargetMethod.Name: "ToString", Arguments.Length: 0 })
        {
            return;
        }

        if (invocation.Instance?.Type?.Name is
                "List" or
                "HashSet" or
                "Dictionary" or
                "Queue" or
                "Stack" or
                "SortedDictionary" or
                "SortedList" or
                "SortedSet" or
                "LinkedList" or
                "PriorityQueue" or
                "IEnumerable" or
                "IList" or
                "ISet" or
                "IDictionary" or
                "ICollection" or
                "IReadOnlyCollection" or
                "IReadOnlyList" or
                "IReadOnlySet" or
                "IReadOnlyDictionary" or
                "IImmutableList" or
                "ImmutableArray" or
                "IImmutableStack" or
                "IImmutableSet" or
                "IImmutableQueue" or
                "IImmutableDictionary" or
                "ImmutableHashSet" or
                "ImmutableList" or
                "ImmutableQueue" or
                "ImmutableSortedDictionary" or
                "ImmutableDictionary" or
                "ImmutableSortedSet" or
                "ImmutableStack")
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
        }
    }
}