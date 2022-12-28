using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpContextStoredInFieldAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.HttpContextStoredInField,
        "HttpContext was stored in a field",
        "HttpContext was stored in a field. Use IHttpContextAccessor instead",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS038-HttpContextStoredInField.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSymbolAction(AnalyzeCreation, SymbolKind.Field);
    }

    private static void AnalyzeCreation(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;
        if (fieldSymbol.Type is { Name: "HttpContext" } && fieldSymbol.Type.IsDefinedInSystemAssembly())
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, fieldSymbol.Locations[0]));
        }
    }
}