using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SynchronousTaskWaitAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = "Asynchronously wait for task completion using await instead";
        private static readonly string Title = "Asynchronously await tasks instead of blocking them";

        public static DiagnosticDescriptor Rule => new(DiagnosticId.SynchronousTaskWait, Title, Message, Category, Severity, isEnabledByDefault: true);

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

            if (invokedSymbol.Name == "Wait" && invokedSymbol.ContainingType?.Name == "Task")
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation()));
            }
        }
    }
}