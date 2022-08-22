using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExplicitEnumValuesAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.GeneralCategory;
        private static readonly string Message = Resources.ExplicitEnumValuesAnalyzerMessage;
        private static readonly string Title = Resources.ExplicitEnumValuesAnalyzerTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.ExplicitEnumValues, Title, Message, Category, Severity, true);

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
}