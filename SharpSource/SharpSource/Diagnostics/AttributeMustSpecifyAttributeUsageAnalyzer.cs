using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AttributeMustSpecifyAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.AttributeMustSpecifyAttributeUsage,
        "An attribute was defined without specifying the [AttributeUsage]",
        "{0} should specify how the attribute can be used",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS044-AttributeMustSpecifyAttributeUsage.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var attributeSymbol = context.Compilation.GetTypeByMetadataName("System.Attribute");
            var attributeUsageAttributeSymbol = context.Compilation.GetTypeByMetadataName("System.AttributeUsageAttribute");
            if (attributeSymbol is not null && attributeUsageAttributeSymbol is not null)
            {
                context.RegisterSymbolAction(context => AnalyzeSymbol(context, attributeSymbol, attributeUsageAttributeSymbol), SymbolKind.NamedType);
            }
        });
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context, INamedTypeSymbol attributeSymbol, INamedTypeSymbol attributeUsageAttributeSymbol)
    {
        var classSymbol = (INamedTypeSymbol)context.Symbol;
        if (!classSymbol.InheritsFrom(attributeSymbol))
        {
            return;
        }

        var hasAttributeUsage = HasAttributeUsageAttribute(classSymbol, attributeSymbol, attributeUsageAttributeSymbol);
        if (!hasAttributeUsage)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, classSymbol.Locations[0], classSymbol.Name));
        }
    }

    private static bool HasAttributeUsageAttribute(INamedTypeSymbol classSymbol, INamedTypeSymbol attributeSymbol, INamedTypeSymbol attributeUsageAttributeSymbol)
    {
        var currentSymbol = classSymbol;
        while (currentSymbol != default)
        {
            if (currentSymbol.Equals(attributeSymbol, SymbolEqualityComparer.Default))
            {
                return false;
            }

            var hasAttributeUsage = currentSymbol.GetAttributes().Any(a => attributeUsageAttributeSymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default));
            if (hasAttributeUsage)
            {
                return true;
            }

            currentSymbol = currentSymbol.BaseType;
        }

        return false;
    }
}