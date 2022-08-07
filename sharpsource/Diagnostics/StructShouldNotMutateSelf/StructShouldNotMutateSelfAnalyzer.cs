using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.StructShouldNotMutateSelf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StructShouldNotMutateSelfAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.StructsCategory;
        private static readonly string Message = Resources.StructsShouldNotMutateSelfAnalyzerMessage;
        private static readonly string Title = Resources.StructsShouldNotMutateSelfAnalyzerTitle;

        public static DiagnosticDescriptor Rule
            => new(DiagnosticId.StructShouldNotMutateSelf, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            // Looking for
            // this = someValueType;
            var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

            if (assignmentExpression.Left is not ThisExpressionSyntax)
            {
                return;
            }

            var type = context.SemanticModel.GetTypeInfo(assignmentExpression.Left).Type;
            if (type == null)
            {
                return;
            }

            if (!type.IsValueType)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, assignmentExpression.Left.GetLocation(), type.Name));
        }
    }
}