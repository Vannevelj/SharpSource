using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StructWithoutElementaryMethodsOverriddenAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StructWithoutElementaryMethodsOverridden,
        "Structs should implement Equals(), GetHashCode(), and ToString().",
        "Implement Equals(), GetHashCode() and ToString() methods on struct {0}.",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS017-StructWithoutElementaryMethodsOverridden.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var objectSymbol = compilationContext.Compilation.GetSpecialType(SpecialType.System_Object);
            var equalsSymbol = objectSymbol?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
            var getHashCodeSymbol = objectSymbol?.GetMembers("GetHashCode").OfType<IMethodSymbol>().FirstOrDefault();
            var toStringSymbol = objectSymbol?.GetMembers("ToString").OfType<IMethodSymbol>().FirstOrDefault();

            if (equalsSymbol is null || getHashCodeSymbol is null || toStringSymbol is null)
            {
                return;
            }
            compilationContext.RegisterSymbolAction(context => Analyze(context, equalsSymbol, getHashCodeSymbol, toStringSymbol), SymbolKind.NamedType);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, IMethodSymbol equalsSymbol, IMethodSymbol getHashCodeSymbol, IMethodSymbol toStringSymbol)
    {
        var structSymbol = (INamedTypeSymbol)context.Symbol;
        if (structSymbol.TypeKind is not TypeKind.Struct)
        {
            return;
        }

        var equalsImplemented = false;
        var getHashCodeImplemented = false;
        var toStringImplemented = false;

        foreach (var member in structSymbol.GetMembers())
        {
            if (member is not IMethodSymbol method)
            {
                continue;
            }

            if (!method.IsOverride)
            {
                continue;
            }

            method = method.GetBaseDefinition();

            // this will happen if the base class is deleted and there is still a derived class
            if (method == default)
            {
                return;
            }

            if (method.Equals(equalsSymbol, SymbolEqualityComparer.Default) == true)
            {
                equalsImplemented = true;
            }

            if (method.Equals(getHashCodeSymbol, SymbolEqualityComparer.Default) == true)
            {
                getHashCodeImplemented = true;
            }

            if (method.Equals(toStringSymbol, SymbolEqualityComparer.Default) == true)
            {
                toStringImplemented = true;
            }
        }

        if (!equalsImplemented || !getHashCodeImplemented || !toStringImplemented)
        {
            var properties = ImmutableDictionary.CreateRange(new KeyValuePair<string, string?>[]
                {
                    new ("IsEqualsImplemented", equalsImplemented.ToString()),
                    new ("IsGetHashCodeImplemented", getHashCodeImplemented.ToString()),
                    new ("IsToStringImplemented", toStringImplemented.ToString())
                });

            context.ReportDiagnostic(Diagnostic.Create(Rule, structSymbol.Locations[0], properties, structSymbol.Name));
        }
    }
}