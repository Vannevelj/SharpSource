using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(DiagnosticId.ComparingStringsWithoutStringComparison + "CF", LanguageNames.CSharp), Shared]
public class ComparingStringsWithoutStringComparisonCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.ComparingStringsWithoutStringComparison);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root is not CompilationUnitSyntax compilation)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var diagnosticNode = compilation.FindNode(diagnosticSpan);

        var expression = diagnosticNode.FirstAncestorOrSelfOfType(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.IsPatternExpression);
        if (expression == default)
        {
            return;
        }        

        var isOrdinal = diagnostic.Properties["comparison"] == "ordinal";
        var stringComparison = isOrdinal ? "OrdinalIgnoreCase" : "InvariantCultureIgnoreCase";

        context.RegisterCodeFix(
            CodeAction.Create("Use StringComparison.OrdinalIgnoreCase",
                x => expression is BinaryExpressionSyntax binaryExpression
                    ? UseStringComparison(context.Document, compilation, binaryExpression, stringComparison)
                    : UseStringComparison(context.Document, compilation, (IsPatternExpressionSyntax) expression, stringComparison),
                DiagnosticId.ComparingStringsWithoutStringComparison),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create("Use StringComparison.InvariantCultureIgnoreCase",
                x => expression is BinaryExpressionSyntax binaryExpression
                    ? UseStringComparison(context.Document, compilation, binaryExpression, stringComparison)
                    : UseStringComparison(context.Document, compilation, (IsPatternExpressionSyntax)expression, stringComparison),
                DiagnosticId.ComparingStringsWithoutStringComparison),
            diagnostic);
    }

    private static Task<Document> UseStringComparison(Document document, CompilationUnitSyntax root, BinaryExpressionSyntax binaryExpression, string stringComparison)
    {
        var invocationKinds = new[] { SyntaxKind.InvocationExpression, SyntaxKind.ConditionalAccessExpression };
        var newLeftSideExpression = binaryExpression.Left.FirstAncestorOrSelfOfType(invocationKinds)?.RemoveInvocation() ?? binaryExpression.Left;
        var newRightSideExpression = binaryExpression.Right.FirstAncestorOrSelfOfType(invocationKinds)?.RemoveInvocation() ?? binaryExpression.Right;
        var negation = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

        return UseStringComparison(document, root, binaryExpression, newLeftSideExpression, newRightSideExpression, stringComparison, negation);
    }

    private static Task<Document> UseStringComparison(Document document, CompilationUnitSyntax root, IsPatternExpressionSyntax isPatternExpression, string stringComparison)
    {
        var newLeftSideExpression = ( isPatternExpression.Expression.FirstAncestorOrSelfOfType(SyntaxKind.InvocationExpression) as InvocationExpressionSyntax )?.RemoveInvocation() ?? isPatternExpression.Expression;
        var newPattern = isPatternExpression.Pattern;
        var negation = false;
        if (isPatternExpression.Pattern is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern } notPattern)
        {
            negation = true;
            newPattern = notPattern.Pattern;
        }

        return UseStringComparison(document, root, isPatternExpression, newLeftSideExpression, newPattern, stringComparison, negation);
    }

    private static Task<Document> UseStringComparison(Document document, CompilationUnitSyntax root, SyntaxNode expressionToReplace, SyntaxNode firstArgument, SyntaxNode secondArgument, string stringComparison, bool useNegation)
    {
        var negation = useNegation ? "!" : "";
        var newNode = SyntaxFactory.ParseExpression($"{negation}string.Equals({firstArgument}, {secondArgument}, StringComparison.{stringComparison})");
        var newRoot = root.ReplaceNode(expressionToReplace, newNode).AddUsingStatementIfMissing("System");
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}