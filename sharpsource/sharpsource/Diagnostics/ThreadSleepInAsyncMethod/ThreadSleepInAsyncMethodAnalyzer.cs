using System;
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

        public static DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId.ThreadSleepInAsyncMethod, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            var isAsync = method.Modifiers.Contains(SyntaxKind.AsyncKeyword);
            if (isAsync)
            {
                AnalyzeMembers(method, context);
                return;
            }

            var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType);
            var hasTaskReturnType = returnType.Type?.Name == "Task";
            if (hasTaskReturnType)
            {
                AnalyzeMembers(method, context);
                return;
            }
        }

        private void AnalyzeMembers(MethodDeclarationSyntax method, SyntaxNodeAnalysisContext context)
        {
            foreach (var memberAccess in method.Body.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>())
            {
                AnalyzeMember(memberAccess, context);
            }
        }

        private void AnalyzeMember(MemberAccessExpressionSyntax node, SyntaxNodeAnalysisContext context)
        {
            var invokingSymbolIdentifier = node.Expression as IdentifierNameSyntax;
            var invokingSymbolIdentifierName = invokingSymbolIdentifier?.Identifier.ValueText;
            if (invokingSymbolIdentifierName != "Thread")
            {
                return;
            }

            var invokedSymbol = node.Name as IdentifierNameSyntax;
            var invokedSymbolName = invokedSymbol?.Identifier.ValueText;
            if (invokedSymbolName != "Sleep")
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}
