using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ComparingStringsWithoutStringComparisonCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.ComparingStringsWithoutStringComparison);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilation || semanticModel == default)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var diagnosticNode = compilation.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        var expression = diagnosticNode.FirstAncestorOrSelfOfType(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.IsPatternExpression);
        if (expression == default)
        {
            return;
        }

        var isOrdinal = diagnostic.Properties["comparison"] == "ordinal";
        var invokedFunction = diagnostic.Properties["function"];
        var stringComparison = isOrdinal ? "OrdinalIgnoreCase" : "InvariantCultureIgnoreCase";

        if (invokedFunction == default)
        {
            return;
        }

        var message = isOrdinal ? "Use StringComparison.OrdinalIgnoreCase" : "Use StringComparison.InvariantCultureIgnoreCase";

        context.RegisterCodeFix(
            CodeAction.Create(message,
                x => expression is BinaryExpressionSyntax binaryExpression
                    ? UseStringComparison(context.Document, compilation, binaryExpression, stringComparison, invokedFunction, semanticModel)
                    : UseStringComparison(context.Document, compilation, (IsPatternExpressionSyntax)expression, stringComparison, invokedFunction, semanticModel),
                DiagnosticId.ComparingStringsWithoutStringComparison),
            diagnostic);
    }

    private static Task<Document> UseStringComparison(Document document, CompilationUnitSyntax root, BinaryExpressionSyntax binaryExpression, string stringComparison, string invokedFunction, SemanticModel semanticModel)
    {
        var newLeftSideExpression = binaryExpression.Left.RemoveInvocation(typeof(string), invokedFunction, semanticModel, unwrapSuppress: true);
        var newRightSideExpression = binaryExpression.Right.RemoveInvocation(typeof(string), invokedFunction, semanticModel, unwrapSuppress: true);
        var negation = binaryExpression.IsKind(SyntaxKind.NotEqualsExpression);

        return UseStringComparison(document, root, binaryExpression, newLeftSideExpression, newRightSideExpression, stringComparison, negation);
    }

    private static Task<Document> UseStringComparison(Document document, CompilationUnitSyntax root, IsPatternExpressionSyntax isPatternExpression, string stringComparison, string invokedFunction, SemanticModel semanticModel)
    {
        var newLeftSideExpression = isPatternExpression.Expression.RemoveInvocation(typeof(string), invokedFunction, semanticModel, unwrapSuppress: true);
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
        var newRoot = root.ReplaceNode(expressionToReplace, newNode.WithLeadingTrivia(firstArgument.GetLeadingTrivia())).WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, SymbolAnnotation.Create("System.StringComparison"));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}