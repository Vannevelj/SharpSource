using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UnboundedStackallocCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.UnboundedStackalloc);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == default)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var stackallocCreation = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<StackAllocArrayCreationExpressionSyntax>().First();
        var arraySizeIdentifier = stackallocCreation.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().First().Sizes.First();
        var arrayType = stackallocCreation.Type as ArrayTypeSyntax;
        if (arrayType == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Add bounds check",
                x => AddBoundsCheck(context.Document, root, stackallocCreation, arraySizeIdentifier, arrayType), DiagnosticId.UnboundedStackalloc), diagnostic);
    }

    private Task<Document> AddBoundsCheck(Document document, SyntaxNode root, StackAllocArrayCreationExpressionSyntax stackallocCreation, ExpressionSyntax arraySizeIdentifier, ArrayTypeSyntax arrayType)
    {
        var binaryExpression = SyntaxFactory.BinaryExpression(
                    SyntaxKind.LessThanExpression,
                    arraySizeIdentifier,
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(1024)
                    )
                );

        var ternary = SyntaxFactory.ConditionalExpression(
            binaryExpression,
            stackallocCreation,
            SyntaxFactory.ArrayCreationExpression(arrayType)
        );

        var newRoot = root.ReplaceNode(stackallocCreation, ternary);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}