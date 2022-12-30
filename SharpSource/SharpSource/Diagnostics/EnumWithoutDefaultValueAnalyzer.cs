using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumWithoutDefaultValueAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] AcceptedNames = new[] { "Unknown", "None" };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.EnumWithoutDefaultValue,
        "An enum should specify a default value",
        "Enum {0} should specify a default value of 0 as \"Unknown\" or \"None\"",
        Categories.ApiDesign,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS039-EnumWithoutDefaultValue.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = (INamedTypeSymbol)context.Symbol;
        if (symbol.TypeKind != TypeKind.Enum)
        {
            return;
        }

        var membersOfInterest = symbol.GetMembers().Where(en => AcceptedNames.Contains(en.Name)).ToArray();
        if (membersOfInterest.Length == 0)
        {
            Report(symbol, context);
            return;
        }

        if (!membersOfInterest.Any(m => ( (IFieldSymbol)m ).ConstantValue is 0))
        {
            Report(symbol, context);
        }
    }

    private static void Report(INamedTypeSymbol symbol, SymbolAnalysisContext context) => context.ReportDiagnostic(Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name));
}