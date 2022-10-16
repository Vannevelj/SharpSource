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

[ExportCodeFixProvider(DiagnosticId.LockingOnMutableReference + "CF", LanguageNames.CSharp), Shared]
public class LockingOnMutableReferenceCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.LockingOnMutableReference);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == default)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var stackallocCreation = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<StackAllocArrayCreationExpressionSyntax>().First();
        var arraySizeIdentifier = stackallocCreation.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().First().Sizes.First();
        var arrayType = stackallocCreation.Type as ArrayTypeSyntax;
        if (arrayType == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Prevent lock re-assignment",
                x => AddReadonlyModifier(context.Document, root), DiagnosticId.LockingOnMutableReference), diagnostic);
    }

    private Task<Document> AddReadonlyModifier(Document document, SyntaxNode root)
    {
        return Task.FromResult(document);
    }
}