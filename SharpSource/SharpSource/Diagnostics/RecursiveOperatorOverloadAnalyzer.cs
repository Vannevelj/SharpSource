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

        var operatorUsages = body.DescendantTokens().Where(x => x.IsKind(definedToken.Kind())).ToArray();

        if (operatorUsages.Length == 0)
        {
            return;
        }

        var enclosingTypeNode = operatorDeclaration.GetEnclosingTypeNode();
        if (enclosingTypeNode == null)
        {
            return;
        }

        var enclosingSymbol = context.SemanticModel.GetDeclaredSymbol(enclosingTypeNode);
        if (enclosingSymbol == null)
        {
            return;
        }

        var tokensFlagged = new List<SyntaxToken>();

        if (definedToken.IsKind(SyntaxKind.TrueKeyword) || definedToken.IsKind(SyntaxKind.FalseKeyword))
        {
            checkForTrueOrFalseKeyword();
            return;
        }

        foreach (var usage in operatorUsages)
        {
            var surroundingNode = body.FindNode(usage.FullSpan);
            if (surroundingNode == null)
            {
                continue;
            }

            if (surroundingNode is not ExpressionSyntax expression)
            {
                continue;
            }

            switch (expression)
            {
                case BinaryExpressionSyntax binaryExpression:
                    var hasWarned = checkOperatorToken(binaryExpression.OperatorToken, binaryExpression.Left);
                    if (!hasWarned)
                    {
                        checkOperatorToken(binaryExpression.OperatorToken, binaryExpression.Right);
                    }

                    break;

                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    checkOperatorToken(prefixUnaryExpression.OperatorToken, prefixUnaryExpression.Operand);
                    break;

                case PostfixUnaryExpressionSyntax postfixUnaryExpression:
                    checkOperatorToken(postfixUnaryExpression.OperatorToken, postfixUnaryExpression.Operand);
                    break;

                default:
                    continue;
            }
        }

        bool checkOperatorToken(SyntaxToken token, ExpressionSyntax expression)
        {
            if (!token.IsKind(definedToken.Kind()))
            {
                return false;
            }

            var usedType = context.SemanticModel.GetTypeInfo(expression).Type;
            if (usedType == null)
            {
                return false;
            }

            if (!usedType.Equals(enclosingSymbol, SymbolEqualityComparer.Default))
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
                    checkOperatorToken(definedToken, ifCondition.Condition);
                }
            }
            else if (operatorDeclaration.ExpressionBody != default)
            {
                var conditionalExpressions = operatorDeclaration.ExpressionBody.Expression.DescendantNodesAndSelf().OfType<ConditionalExpressionSyntax>().ToArray();
                foreach (var conditionalExpression in conditionalExpressions)
                {
                    checkOperatorToken(definedToken, conditionalExpression.Condition);
                }
            }
        }
    }
}