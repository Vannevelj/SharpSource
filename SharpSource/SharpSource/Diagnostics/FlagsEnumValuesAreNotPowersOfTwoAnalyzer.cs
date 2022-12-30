using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FlagsEnumValuesAreNotPowersOfTwoAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
            DiagnosticId.FlagsEnumValuesAreNotPowersOfTwo,
            "[Flags] enum members need to be either powers of two, or bitwise OR expressions.",
            "Enum {0}.{1} is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.",
            Categories.Correctness,
            DiagnosticSeverity.Error,
            true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS007-FlagsEnumValuesAreNotPowersOfTwo.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EnumDeclaration);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        var hasFlags = enumDeclaration.AttributeLists.GetAttributesOfType(typeof(FlagsAttribute), context.SemanticModel).FirstOrDefault();
        if (hasFlags == default)
        {
            return;
        }

        var enumName = context.SemanticModel.GetDeclaredSymbol(enumDeclaration)?.Name;
        var enumMemberDeclarations = enumDeclaration
            .ChildNodes()
            .OfType<EnumMemberDeclarationSyntax>(SyntaxKind.EnumMemberDeclaration)
            .ToArray();

        foreach (var member in enumMemberDeclarations)
        {
            if (member?.EqualsValue?.Value is LiteralExpressionSyntax literal && !literal.IsKind(SyntaxKind.CharacterLiteralExpression))
            {
                var constantValue = context.SemanticModel.GetConstantValue(literal);
                if (constantValue is { Value: null })
                {
                    continue;
                }

                if (!IsPowerOfTwo(constantValue.Value))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, member.EqualsValue.Value.GetLocation(), enumName, member.Identifier.ValueText));
                }
            }
        }
    }

    private static bool IsPowerOfTwo(object? value) =>
        value switch
        {
            int v => ( v & ( v - 1 ) ) == 0,
            uint v => ( v & ( v - 1 ) ) == 0,
            byte v => ( v & ( v - 1 ) ) == 0,
            sbyte v => ( v & ( v - 1 ) ) == 0,
            long v => ( v & ( v - 1 ) ) == 0,
            ulong v => ( v & ( v - 1 ) ) == 0,
            short v => ( v & ( v - 1 ) ) == 0,
            ushort v => ( v & ( v - 1 ) ) == 0,
            _ => false
        };
}