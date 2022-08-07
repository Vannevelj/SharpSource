using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.DateTimeNow
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeNowAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.GeneralCategory;
        private static readonly string Message = Resources.DateTimeNowAnalyzerMessage;
        private static readonly string Title = Resources.DateTimeNowAnalyzerTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.DateTimeNow, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var expression = (MemberAccessExpressionSyntax)context.Node;

            if (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol is INamedTypeSymbol symbol &&
                symbol.SpecialType == SpecialType.System_DateTime &&
                expression.Name.Identifier.ValueText == "Now")
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}