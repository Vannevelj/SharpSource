using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(DiagnosticId.UnboundedStackalloc + "CF", LanguageNames.CSharp), Shared]
public class UnboundedStackallocCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.UnboundedStackalloc);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        if (root == default)
        {
            return;
        }

        var statement = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<StackAllocArrayCreationExpressionSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create("Add bounds check",
                x => AddBoundsCheck(context.Document, root, statement), DiagnosticId.UnboundedStackalloc), diagnostic);
    }

    private Task<Document> AddBoundsCheck(Document document, SyntaxNode root, StackAllocArrayCreationExpressionSyntax statement)
    {
        var newRoot = root.ReplaceNode(statement, SyntaxFactory.ParseExpression("System.DateTime.UtcNow").WithAdditionalAnnotations(Simplifier.Annotation));
        return Task.FromResult(document.WithSyntaxRoot(root));
    }
}