using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleFromBodyParametersAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.MultipleFromBodyParameters,
        "A method was defined with multiple [FromBody] parameters but ASP.NET only supports a single one.",
        "Method {0} specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS043-MultipleFromBodyParameters.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var fromBodySymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.FromBodyAttribute");
            if (fromBodySymbol is not null)
            {
                compilationContext.RegisterSymbolAction(context => AnalyzeMethod(context, fromBodySymbol), SymbolKind.Method);
                compilationContext.RegisterSyntaxNodeAction(context => AnalyzeLambda(context, fromBodySymbol), SyntaxKind.ParenthesizedLambdaExpression);
            }
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol fromBodySymbol)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        var attributesOnParameters = methodSymbol.Parameters
                                                 .SelectMany(p => p.GetAttributes())
                                                 .Count(a => fromBodySymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default));

        if (attributesOnParameters > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name));
        }
    }

    private static void AnalyzeLambda(SyntaxNodeAnalysisContext context, INamedTypeSymbol fromBodySymbol)
    {
        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;
        var fromBodyCount = 0;

        foreach (var parameter in lambda.ParameterList.Parameters)
        {
            foreach (var attributeList in parameter.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeSymbol = context.SemanticModel.GetTypeInfo(attribute).Type;
                    if (fromBodySymbol.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                    {
                        fromBodyCount++;
                        if (fromBodyCount > 1)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation(), "lambda expression"));
                            return;
                        }
                    }
                }
            }
        }
    }
}