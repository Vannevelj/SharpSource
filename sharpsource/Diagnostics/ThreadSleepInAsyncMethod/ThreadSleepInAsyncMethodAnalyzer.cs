using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.ThreadSleepInAsyncMethod
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThreadSleepInAsyncMethodAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = Resources.ThreadSleepInAsyncMethodMessage;
        private static readonly string Title = Resources.ThreadSleepInAsyncMethodTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.ThreadSleepInAsyncMethod, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            var isAsync = method.Modifiers.Contains(SyntaxKind.AsyncKeyword);
            if (isAsync)
            {
                AnalyzeMembers(method, context, isAsync);
                return;
            }

            var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType);
            var hasTaskReturnType = returnType.Type?.Name == "Task";
            if (hasTaskReturnType)
            {
                AnalyzeMembers(method, context, isAsync);
                return;
            }
        }

        private void AnalyzeMembers(MethodDeclarationSyntax method, SyntaxNodeAnalysisContext context, bool isAsync)
        {
            foreach (var invocation in method.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                switch (invocation.Expression)
                {
                    case MemberAccessExpressionSyntax memberAccess:
                        IsAccessingThreadDotSleep(memberAccess.Name, context, isAsync);
                        break;
                    case IdentifierNameSyntax identifierName:
                        IsAccessingThreadDotSleep(identifierName, context, isAsync);
                        break;
                }
            }
        }

        private void IsAccessingThreadDotSleep(SimpleNameSyntax invokedFunction, SyntaxNodeAnalysisContext context, bool isAsync)
        {
            var invokedSymbol = context.SemanticModel.GetSymbolInfo(invokedFunction).Symbol;
            if (invokedSymbol == null)
            {
                return;
            }

            var isAccessedFunctionCorrect = invokedSymbol.Name == "Sleep";
            var isAccessedTypeCorrect = invokedSymbol.ContainingType?.Name == "Thread";

            if (isAccessedFunctionCorrect && isAccessedTypeCorrect)
            {
                var dic = ImmutableDictionary.CreateBuilder<string, string>();
                dic.Add("isAsync", isAsync.ToString());

                var invocationNode = invokedFunction.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocationNode.GetLocation(), dic.ToImmutable(), null));
            }
        }
    }
}