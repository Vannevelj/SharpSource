using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.StructDeclaration);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var objectSymbol = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Object);
        IMethodSymbol? objectEquals = null;
        IMethodSymbol? objectGetHashCode = null;
        IMethodSymbol? objectToString = null;

        foreach (var symbol in objectSymbol.GetMembers())
        {
            if (symbol is not IMethodSymbol)
            {
                continue;
            }

            var method = (IMethodSymbol)symbol;
            if (method is { MetadataName: nameof(Equals), Parameters.Length: 1 })
            {
                objectEquals = method;
            }

            if (method is { MetadataName: nameof(GetHashCode), Parameters.Length: 0 })
            {
                objectGetHashCode = method;
            }

            if (method is { MetadataName: nameof(ToString), Parameters.Length: 0 })
            {
                objectToString = method;
            }
        }

        var structDeclaration = (StructDeclarationSyntax)context.Node;
        var structSymbol = context.SemanticModel.GetDeclaredSymbol(structDeclaration);
        if (structSymbol == default)
        {
            return;
        }

        var equalsImplemented = false;
        var getHashCodeImplemented = false;
        var toStringImplemented = false;

        foreach (var node in structSymbol.GetMembers())
        {
            if (node is not IMethodSymbol method)
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

            if (method.Equals(objectEquals, SymbolEqualityComparer.Default) == true)
            {
                equalsImplemented = true;
            }

            if (method.Equals(objectGetHashCode, SymbolEqualityComparer.Default) == true)
            {
                getHashCodeImplemented = true;
            }

            if (method.Equals(objectToString, SymbolEqualityComparer.Default) == true)
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

            context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), properties, structDeclaration.Identifier));
        }
    }
}