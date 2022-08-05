using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.ThrowNull
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowNullAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Error;

        private static readonly string Category = Resources.ExceptionsCategory;
        private static readonly string Message = Resources.ThrowNullMessage;
        private static readonly string Title = Resources.ThrowNullMessage;

        public static DiagnosticDescriptor Rule
            => new DiagnosticDescriptor(DiagnosticId.ThrowNull, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ThrowStatement);

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;
            if (throwStatement.Expression == null)
            {
                return;
            }

            var throwValue = context.SemanticModel.GetConstantValue(throwStatement.Expression);
            if (throwValue.HasValue && throwValue.Value == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, throwStatement.Expression.GetLocation()));
            }
        }
    }
}
