using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnnecessaryEnumerableMaterializationAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "{0} is unnecessarily materializing the IEnumerable and can be omitted";
    private static readonly string Title = "An IEnumerable was materialized before a deferred execution call";

    private static readonly HashSet<string> MaterializingOperations = new(){
        "ToList", "ToArray", "ToHashSet"
    };

    private static readonly HashSet<string> DeferredExecutionOperations = new() {
        "Select", "SelectMany", "Take", "Skip", "TakeWhile", "SkipWhile", "SkipLast", "Where", "GroupBy", "GroupJoin", "OrderBy", "OrderByDescending",
        "Union", "UnionBy", "Zip", "Reverse", "Join", "OfType", "Intersect", "IntersectBy", "Except", "ExceptBy", "Distinct",
        "DistinctBy", "DefaultIfEmpty", "Concat", "Cast"
    };

    public static DiagnosticDescriptor Rule => new(DiagnosticId.UnnecessaryEnumerableMaterialization, Title, Message, Categories.Performance, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var expression = (MemberAccessExpressionSyntax)context.Node;
        if (expression.Expression is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        foreach (var materializingOperation in MaterializingOperations)
        {
            var invokedFunction = expression.Name.Identifier.ValueText;
            var isSuspiciousInvocation = DeferredExecutionOperations.Contains(invokedFunction) || MaterializingOperations.Contains(invokedFunction);
            if (invocation.IsAnInvocationOf(typeof(Enumerable), materializingOperation, context.SemanticModel) &&
                isSuspiciousInvocation)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("operation", materializingOperation);
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), properties.ToImmutable(), $"{materializingOperation}"));
                return;
            }
        }
    }
}