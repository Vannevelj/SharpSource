using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.CorrectTPLMethodsInAsyncContext
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncOverloadsAvailableAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = Resources.AsyncOverloadsAvailableMessage;
        private static readonly string Title = Resources.AsyncOverloadsAvailableTitle;

        public static DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId.AsyncOverloadsAvailable, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    CheckIfOverloadAvailable(memberAccess.Name as IdentifierNameSyntax, context);
                }
            }
        }

        private void CheckIfOverloadAvailable(IdentifierNameSyntax invokedFunction, SyntaxNodeAnalysisContext context)
        {
            var invokedSymbol = context.SemanticModel.GetSymbolInfo(invokedFunction).Symbol;
            if (invokedSymbol == null)
            {
                return;
            }

            var invokedMethod = invokedSymbol.Name;
            var invokedType = invokedSymbol.ContainingType?.Name;

            var hasOverload = invokedSymbol.ContainingType.MemberNames.Any(x => x == $"{invokedMethod}Async");

            if (hasOverload)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invokedFunction.GetLocation(), $"{invokedType}.{invokedMethod}"));
            }
        }
    }
}
