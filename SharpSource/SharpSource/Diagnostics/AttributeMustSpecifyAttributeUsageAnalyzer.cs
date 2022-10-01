using System;
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
public class AttributeMustSpecifyAttributeUsageAnalyzer : DiagnosticAnalyzer
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
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null || !classSymbol.InheritsFrom(typeof(Attribute)))
        {
            return;
        }

        var attributeUsage = GetAttributeUsageAttribute(classSymbol);
        if (attributeUsage == default)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
        }
    }

    private static INamedTypeSymbol? GetAttributeUsageAttribute(INamedTypeSymbol classSymbol)
    {
        var currentSymbol = classSymbol;
        while (currentSymbol != default)
        {
            if (currentSymbol.Name == "Attribute" && currentSymbol.IsDefinedInSystemAssembly())
            {
                return default;
            }

            var attributes = currentSymbol.GetAttributes().Select(a => a.AttributeClass);
            var attribute = attributes.GetAttributesOfType(typeof(AttributeUsageAttribute)).FirstOrDefault();
            if (attribute != default)
            {
                return attribute;
            }

            currentSymbol = currentSymbol.BaseType;
        }

        return default;
    }
}