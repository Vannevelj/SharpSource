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

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var lockStatement = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<LockStatementSyntax>().First();
        var semanticModel = await context.Document.GetSemanticModelAsync();
        var referencedSymbol = semanticModel.GetSymbolInfo(lockStatement.Expression).Symbol;
        if (referencedSymbol == default)
        {
            return;
        }

        var fieldReference = referencedSymbol.DeclaringSyntaxReferences.Single();

        if (fieldReference.SyntaxTree != await context.Document.GetSyntaxTreeAsync() || ( await fieldReference.GetSyntaxAsync() ).FirstAncestorOrSelfOfType(SyntaxKind.FieldDeclaration) is not FieldDeclarationSyntax fieldSyntaxNode)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Prevent lock re-assignment",
                x => AddReadonlyModifier(context.Document, root, fieldSyntaxNode), DiagnosticId.LockingOnMutableReference), diagnostic);
    }

    private static Task<Document> AddReadonlyModifier(Document document, SyntaxNode root, FieldDeclarationSyntax fieldDeclaration)
    {
        var newFieldDeclaration = fieldDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}