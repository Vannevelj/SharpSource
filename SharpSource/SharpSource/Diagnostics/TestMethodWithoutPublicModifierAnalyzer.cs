using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestMethodWithoutPublicModifierAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.TestMethodWithoutPublicModifier,
        "Verifies whether a test method has the public modifier.",
        "Test method \"{0}\" is not public.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS020-TestMethodWithoutPublicModifier.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var testMethodAttributeSymbols = ImmutableArray.Create(
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.FactAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.TheoryAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("NUnit.Framework.TestAttribute")
            );

            compilationContext.RegisterSymbolAction(context => Analyze(context, testMethodAttributeSymbols), SymbolKind.Method);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol?> testMethodAttributeSymbols)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.DeclaredAccessibility == Accessibility.Public)
        {
            return;
        }

        var attributes = method.GetAttributes();
        foreach (var attribute in attributes)
        {
            var attributeType = attribute.AttributeClass;
            while (attributeType is not null)
            {
                if (testMethodAttributeSymbols.Any(symbol => attributeType.Equals(symbol, SymbolEqualityComparer.Default)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
                    return;
                }

                attributeType = attributeType.BaseType;
            }
        }
    }
}