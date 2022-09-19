using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComparingStringsWithoutStringComparisonAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ComparingStringsWithoutStringComparison,
        "A string is being compared through allocating a new string, e.g. using ToLower() or ToUpperInvariant(). Use a case-insensitive comparison instead which does not allocate.",
        "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS049-ComparingStringsWithoutStringComparison.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.IsPatternExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var expressionsToCheck = context.Node switch
        {
            BinaryExpressionSyntax binaryExpression => new[] { binaryExpression.Left, binaryExpression.Right },
            IsPatternExpressionSyntax isPatternExpression => new[] { isPatternExpression.Expression },
            _ => Array.Empty<ExpressionSyntax>()
        };

        foreach (var expression in expressionsToCheck)
        {
            var capitalizationFunction = StringCapitalizationFunction(expression, context.SemanticModel);
            if (capitalizationFunction != CapitalizationFunction.None)
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("comparison", capitalizationFunction == CapitalizationFunction.Ordinal ? "ordinal" : "invariant");
                context.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation(), properties.ToImmutable()));
                break;
            }
        }
    }

    private static CapitalizationFunction StringCapitalizationFunction(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (expression.IsAnInvocationOf(typeof(string), "ToLower", semanticModel) ||
            expression.IsAnInvocationOf(typeof(string), "ToUpper", semanticModel)) {
            return CapitalizationFunction.Ordinal;
        }

        if (expression.IsAnInvocationOf(typeof(string), "ToLowerInvariant", semanticModel) ||
            expression.IsAnInvocationOf(typeof(string), "ToUpperInvariant", semanticModel)) {
            return CapitalizationFunction.Invariant;
        }

        return CapitalizationFunction.None;
    }
}

internal enum CapitalizationFunction
{
    None = 0,
    Ordinal = 1,
    Invariant = 2
}