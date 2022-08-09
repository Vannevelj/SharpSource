using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Package;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.NewGuid
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NewGuidAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Error;

        private static readonly string Category = Resources.GeneralCategory;
        private static readonly string Message = Resources.NewGuidAnalyzerMessage;
        private static readonly string Title = Resources.NewGuidAnalyzerTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.NewGuid, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var expression = (ObjectCreationExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(expression.Type).Symbol;

            if (symbol != null &&
                symbol.Name == "Guid" &&
                expression.ArgumentList?.Arguments.Any() != true &&
                ( symbol.ContainingAssembly.Name == "mscorlib" || symbol.ContainingAssembly.Name == "System.Runtime" ))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }
    }
}