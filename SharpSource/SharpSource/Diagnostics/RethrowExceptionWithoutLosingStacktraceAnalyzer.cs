using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RethrowExceptionWithoutLosingStacktraceAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;
        private static readonly string Category = Resources.ExceptionsCategory;
        private static readonly string Message = Resources.RethrowExceptionWithoutLosingStacktraceAnalyzerMessage;
        private static readonly string Title = Resources.RethrowExceptionWithoutLosingStacktraceAnalyzerTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.RethrowExceptionWithoutLosingStacktrace, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ThrowStatement);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;

            if (throwStatement.Expression is not IdentifierNameSyntax throwIdentifierSyntax)
            {
                return;
            }

            var catchClause = throwStatement.Ancestors().OfType<CatchClauseSyntax>(SyntaxKind.CatchClause).FirstOrDefault();

            // Code is in an incomplete state (user is typing the catch clause but hasn't typed the identifier yet)
            var exceptionIdentifier = catchClause?.Declaration?.Identifier;
            if (exceptionIdentifier == null)
            {
                return;
            }

            var catchClauseIdentifier = exceptionIdentifier.Value.ValueText;
            var thrownIdentifier = throwIdentifierSyntax.Identifier.ValueText;

            if (catchClauseIdentifier == thrownIdentifier)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, throwStatement.GetLocation()));
            }
        }
    }
}