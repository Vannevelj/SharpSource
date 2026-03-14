using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RecursiveOperatorOverloadAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.RecursiveOperatorOverload,
        "Recursively using overloaded operator",
        "Recursively using overloaded operator",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS012-RecursiveOperatorOverload.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.OperatorDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;
        var definedToken = operatorDeclaration.OperatorToken;

        SyntaxNode? body = operatorDeclaration.Body != default ? operatorDeclaration.Body : operatorDeclaration.ExpressionBody != default ? operatorDeclaration.ExpressionBody : default;
        if (body == default)
        {
            return;
        }

        var currentOperatorSymbol = context.SemanticModel.GetDeclaredSymbol(operatorDeclaration);
        if (currentOperatorSymbol == null)
        {
            return;
        }

        var enclosingTypeNode = operatorDeclaration.GetEnclosingTypeNode();
        var enclosingSymbol = enclosingTypeNode != null ? context.SemanticModel.GetDeclaredSymbol(enclosingTypeNode) : null;
        if (enclosingSymbol is null)
        {
            return;
        }

        var tokensFlagged = new List<SyntaxToken>();

        if (definedToken.IsKind(SyntaxKind.TrueKeyword) || definedToken.IsKind(SyntaxKind.FalseKeyword))
        {
            checkForTrueOrFalseKeyword();
            return;
        }

        foreach (var expression in body.DescendantNodesAndSelf().OfType<ExpressionSyntax>())
        {
            switch (expression)
            {
                case BinaryExpressionSyntax binaryExpression:
                    checkOperatorExpression(binaryExpression, binaryExpression.OperatorToken);
                    break;

                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    checkOperatorExpression(prefixUnaryExpression, prefixUnaryExpression.OperatorToken);
                    break;

                case PostfixUnaryExpressionSyntax postfixUnaryExpression:
                    checkOperatorExpression(postfixUnaryExpression, postfixUnaryExpression.OperatorToken);
                    break;

                default:
                    continue;
            }
        }

        bool checkOperatorExpression(ExpressionSyntax expression, SyntaxToken token)
        {
            var invokedOperatorSymbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
            if (invokedOperatorSymbol == null)
            {
                return false;
            }

            if (!invokedOperatorSymbol.Equals(currentOperatorSymbol, SymbolEqualityComparer.Default))
            {
                return false;
            }

            if (!tokensFlagged.Contains(token))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation()));
                tokensFlagged.Add(token);
            }

            return true;
        }

        void checkForTrueOrFalseKeyword()
        {
            if (operatorDeclaration.Body != default)
            {
                var ifConditions = operatorDeclaration.Body.DescendantNodes().OfType<IfStatementSyntax>().ToArray();
                foreach (var ifCondition in ifConditions)
                {
                    checkOperatorCondition(ifCondition.Condition);
                }
            }
            else if (operatorDeclaration.ExpressionBody != default)
            {
                var conditionalExpressions = operatorDeclaration.ExpressionBody.Expression.DescendantNodesAndSelf().OfType<ConditionalExpressionSyntax>().ToArray();
                foreach (var conditionalExpression in conditionalExpressions)
                {
                    checkOperatorCondition(conditionalExpression.Condition);
                }
            }

            void checkOperatorCondition(ExpressionSyntax expression)
            {
                var usedType = context.SemanticModel.GetTypeInfo(expression).Type;
                if (usedType == null)
                {
                    return;
                }

                if (!usedType.Equals(enclosingSymbol, SymbolEqualityComparer.Default))
                {
                    return;
                }

                if (!tokensFlagged.Contains(definedToken))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, definedToken.GetLocation()));
                    tokensFlagged.Add(definedToken);
                }
            }
        }
    }
}