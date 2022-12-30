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
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.DeclaredAccessibility != Accessibility.Public && method.GetAttributes().Any(
            a => a.AttributeClass?.Name is "TestAttribute" or "TestMethodAttribute" or "FactAttribute" or "TheoryAttribute"))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0],
                method.Name));
        }
    }
}