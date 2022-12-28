using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        var attributesOnParameters = methodSymbol.Parameters
                                                 .SelectMany(p => p.GetAttributes())
                                                 .Count(a => a.AttributeClass?.MetadataName is "FromBody" or "FromBodyAttribute" && a.AttributeClass?.IsDefinedInSystemAssembly() == true);

        if (attributesOnParameters > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name));
        }
    }
}