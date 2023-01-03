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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var httpContextSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpContext");
            var httpContextAccessorSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.IHttpContextAccessor");
            if (httpContextSymbol is not null && httpContextAccessorSymbol is not null)
            {
                compilationContext.RegisterSymbolAction(context => AnalyzeCreation(context, httpContextSymbol), SymbolKind.Field);
            }
        });
    }

    private static void AnalyzeCreation(SymbolAnalysisContext context, INamedTypeSymbol httpContextSymbol)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;
        if (httpContextSymbol.Equals(fieldSymbol.Type, SymbolEqualityComparer.Default))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, fieldSymbol.Locations[0]));
        }
    }
}