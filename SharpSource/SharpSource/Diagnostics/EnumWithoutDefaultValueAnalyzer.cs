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
public class EnumWithoutDefaultValueAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Enum {0} should specify a default value of 0 as \"Unknown\" or \"None\"";
    private static readonly string Title = "An enum should specify a default value";

    private static readonly string[] AcceptedNames = new[] { "Unknown", "None" };

    public static DiagnosticDescriptor Rule => new(DiagnosticId.EnumWithoutDefaultValue, Title, Message, Categories.Enums, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.EnumDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        var enumMembers = enumDeclaration.DescendantNodes().OfType<EnumMemberDeclarationSyntax>();

        var membersOfInterest = enumMembers.Where(en => AcceptedNames.Contains(en.Identifier.ValueText)).ToArray();
        if (membersOfInterest.Length == 0)
        {
            Report(enumDeclaration.Identifier, context);
            return;
        }

        var membersWithZeroValue = membersOfInterest.Where(m => context.SemanticModel.GetDeclaredSymbol(m).ConstantValue.Equals(0));
        if (!membersWithZeroValue.Any())
        {
            Report(enumDeclaration.Identifier, context);
        }
    }

    private static void Report(SyntaxToken identifier, SyntaxNodeAnalysisContext context) => context.ReportDiagnostic(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
}