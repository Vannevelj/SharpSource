using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExplicitEnumValuesAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ExplicitEnumValues,
        "An enum should explicitly specify its values",
        "Option {0} on enum {1} should explicitly specify its value",
        Categories.ApiDesign,
        DiagnosticSeverity.Info,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS036-ExplicitEnumValues.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EnumMemberDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var declaration = (EnumMemberDeclarationSyntax)context.Node;

        var valueClause = declaration.EqualsValue;
        if (valueClause == null)
        {
            var option = declaration.Identifier.ValueText;
            var enumName = declaration.FirstAncestorOrSelf<EnumDeclarationSyntax>()?.Identifier.ValueText;
            context.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), option, enumName));
        }
    }
}