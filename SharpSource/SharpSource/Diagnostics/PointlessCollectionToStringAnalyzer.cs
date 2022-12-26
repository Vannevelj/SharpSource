using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PointlessCollectionToStringAnalyzer : DiagnosticAnalyzer
{
    // private static readonly

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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // We only care about .ToString() calls without arguments
        // We can't use the invokedType return value because the function is System.Object.ToString() but we want the concrete type instead
        var (_, invokedMethod) = invocation.GetInvocation(context.SemanticModel);
        if (invokedMethod == default || invokedMethod.Name != "ToString" || invocation is { ArgumentList: { Arguments: { Count: > 0 } } })
        {
            return;
        }

        var invokedType = invocation.GetConcreteTypeOfInvocation(context.SemanticModel);
        if (invokedType is
            {
                Name:
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
                    "IEnumerable"
            })
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Expression.GetLocation()));
        }
    }
}