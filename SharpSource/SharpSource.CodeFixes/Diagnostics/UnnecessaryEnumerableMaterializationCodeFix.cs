using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(DiagnosticId.UnnecessaryEnumerableMaterialization + "CF", LanguageNames.CSharp), Shared]
public class UnnecessaryEnumerableMaterializationCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create($"Remove unnecessary {diagnostic.Properties["operation"]} call",
                x => RemoveMaterialization(context.Document, memberAccess, root, diagnostic, x),
                UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id),
            diagnostic);
    }

    private Task<Document> RemoveMaterialization(Document document, InvocationExpressionSyntax invocation, SyntaxNode root, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var surroundingMemberAccess = invocation.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
        var nestedMemberAccess = invocation.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
        if (nestedMemberAccess == null)
        {
            return Task.FromResult(document);
        }

        var newRoot = root.ReplaceNode(surroundingMemberAccess.Expression, nestedMemberAccess.Expression);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}