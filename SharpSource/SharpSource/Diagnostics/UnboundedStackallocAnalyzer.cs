using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnboundedStackallocAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.UnboundedStackalloc,
        "An array is stack allocated without checking whether the length is within reasonable bounds. This can result in performance degradations and security risks",
        "An array is stack allocated without checking the length. Explicitly check the length against a constant value",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.StackAllocArrayCreationExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var stackallocExpression = (StackAllocArrayCreationExpressionSyntax)context.Node;
        var arrayRank = stackallocExpression.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().FirstOrDefault();
        if (arrayRank == default)
        {
            return;
        }

        var sizeExpression = arrayRank.Sizes.FirstOrDefault();
        if (sizeExpression == default)
        {
            return;
        }

        var hasConstantValue = context.SemanticModel.GetConstantValue(sizeExpression).HasValue;
        if (hasConstantValue)
        {
            return;
        }

        var parentContext = stackallocExpression.FirstAncestorOfType(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.LocalFunctionStatement,
            SyntaxKind.GlobalStatement);
        var binaryExpressions = parentContext switch
        {
            MethodDeclarationSyntax method when method.ExpressionBody is not null => method.ExpressionBody.Expression.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>(),
            MethodDeclarationSyntax method when method.Body is not null => method.Body.Statements.SelectMany(st => st.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()),
            LocalFunctionStatementSyntax localFunction when localFunction.ExpressionBody is not null => localFunction.ExpressionBody.Expression.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>(),
            LocalFunctionStatementSyntax localFunction when localFunction.Body is not null => localFunction.Body.Statements.SelectMany(st => st.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>()),
            GlobalStatementSyntax globalStatement => globalStatement.Statement.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>(),
            _ => default
        };

        var sizeExpressionSymbol = context.SemanticModel.GetSymbolInfo(sizeExpression).Symbol;
        if (sizeExpressionSymbol == default)
        {
            return;
        }

        foreach (var binaryExpression in binaryExpressions ?? Enumerable.Empty<BinaryExpressionSyntax>())
        {
            var leftSymbol = context.SemanticModel.GetSymbolInfo(binaryExpression.Left).Symbol;
            var rightSymbol = context.SemanticModel.GetSymbolInfo(binaryExpression.Right).Symbol;
            if (sizeExpressionSymbol.Equals(leftSymbol, SymbolEqualityComparer.Default) ||
                sizeExpressionSymbol.Equals(rightSymbol, SymbolEqualityComparer.Default))
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, arrayRank.GetLocation()));
    }
}