using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ConcurrentDictionaryEmptyCheckCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConcurrentDictionaryEmptyCheckAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var statement = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var isBinaryCheck = diagnostic.Properties.GetValueOrDefault("isBinaryCheck") == "true";
        var binaryOperandOfInterest = diagnostic.Properties.GetValueOrDefault("binaryOperandOfInterest");
        var mustInvert = diagnostic.Properties.GetValueOrDefault("mustInvert") == "true";

        if (isBinaryCheck)
        {
            var binarySyntax = (BinaryExpressionSyntax)statement;
            var memberExpression = binaryOperandOfInterest == "left" ? binarySyntax.Left : binarySyntax.Right;
            switch (memberExpression)
            {
                case MemberAccessExpressionSyntax memberAccessExpression:
                    {
                        registerCodeFix(memberAccessExpression, nodeToReplace: binarySyntax);
                        return;
                    }
                case InvocationExpressionSyntax invocationExpression when invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression:
                    {
                        registerCodeFix(memberAccessExpression, nodeToReplace: binarySyntax);
                        return;
                    }
                default:
                    return;
            }
        }
        else
        {
            var invocation = (InvocationExpressionSyntax)statement;
            registerCodeFix(invocation, nodeToReplace: invocation);
        }

        void registerCodeFix(ExpressionSyntax expression, SyntaxNode nodeToReplace)
        {
            var expressionToUse = expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax } invocation
                ? (MemberAccessExpressionSyntax)invocation.Expression
                : expression is MemberAccessExpressionSyntax memberAccesExpression
                    ? memberAccesExpression
                    : null;

            if (expressionToUse is not null)
            {
                context.RegisterCodeFix(CodeAction.Create(
                    "Is IsEmpty",
                    x => UseIsEmpty(context.Document, root, expressionToUse, nodeToReplace, mustInvert),
                    ConcurrentDictionaryEmptyCheckAnalyzer.Rule.Id),
                    diagnostic);
            }
        }
    }

    private static Task<Document> UseIsEmpty(Document document, SyntaxNode root, MemberAccessExpressionSyntax memberAccessExpression, SyntaxNode nodeToReplace, bool mustInvert)
    {
        var newExpression = (ExpressionSyntax)memberAccessExpression.WithName(SyntaxFactory.IdentifierName("IsEmpty"));
        if (mustInvert)
        {
            newExpression = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, newExpression);
        }
        var newRoot = root.ReplaceNode(nodeToReplace, newExpression);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}