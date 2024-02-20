using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EqualsAndGetHashcodeNotImplementedTogetherAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.EqualsAndGetHashcodeNotImplementedTogether,
        "Implement Equals() and GetHashcode() together.",
        "Equals() and GetHashcode() must be implemented together on {0}",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS005-EqualsAndGetHashcodeNotImplementedTogether.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(context =>
        {
            var objectSymbol = context.Compilation.GetSpecialType(SpecialType.System_Object);
            var objectEquals = objectSymbol.GetMembers(nameof(Equals)).OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 1);
            var objectGetHashCode = objectSymbol.GetMembers(nameof(GetHashCode)).OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.IsEmpty);

            context.RegisterSymbolAction(context =>
            {
                var classSymbol = (INamedTypeSymbol)context.Symbol;
                if (classSymbol.TypeKind != TypeKind.Class)
                {
                    return;
                }

                var equalsImplemented = false;
                var getHashcodeImplemented = false;

                foreach (var node in classSymbol.GetMembers())
                {
                    if (node is not IMethodSymbol method)
                    {
                        continue;
                    }

                    method = method.GetBaseDefinition();

                    if (method.Equals(objectEquals, SymbolEqualityComparer.Default))
                    {
                        equalsImplemented = true;
                    }
                    else if (method.Equals(objectGetHashCode, SymbolEqualityComparer.Default))
                    {
                        getHashcodeImplemented = true;
                    }
                }

                if (equalsImplemented ^ getHashcodeImplemented)
                {
                    var properties = ImmutableDictionary<string, string?>.Empty.Add("IsEqualsImplemented", equalsImplemented.ToString());
                    context.ReportDiagnostic(Diagnostic.Create(Rule, classSymbol.Locations[0], properties, classSymbol.Name));
                }
            }, SymbolKind.NamedType);
        });
    }
}