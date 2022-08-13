using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AccessingTaskResultWithoutAwaitAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = Resources.AccessingTaskResultWithoutAwaitAnalyzerMessage;
        private static readonly string Title = Resources.AccessingTaskResultWithoutAwaitAnalyzerTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.AccessingTaskResultWithoutAwait, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            var invokedSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (invokedSymbol == null)
            {
                return;
            }

            var enclosingLambda = memberAccess.FirstAncestorOrSelf<LambdaExpressionSyntax>();
            if (enclosingLambda != null)
            {
                if (enclosingLambda.AsyncKeyword == default)
                {
                    return;
                }
            }
            else
            {
                var enclosingMethod = memberAccess.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                if (enclosingMethod == null)
                {
                    return;
                }

                if (!enclosingMethod.Modifiers.Any(SyntaxKind.AsyncKeyword))
                {
                    return;
                }
            }

            if (invokedSymbol.Name == "Result" && invokedSymbol.ContainingType?.Name == "Task")
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation()));
            }
        }
    }
}