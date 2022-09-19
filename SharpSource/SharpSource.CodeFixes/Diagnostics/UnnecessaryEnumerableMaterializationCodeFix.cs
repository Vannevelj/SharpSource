using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
        var operation = diagnostic.Properties["operation"];
        var semanticModel = await context.Document.GetSemanticModelAsync();

        if (root == default || operation == default || semanticModel == default)
        {
            return;
        }

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var invocations = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>();
        var invocation = invocations.First(x => x.IsAnInvocationOf(typeof(Enumerable), operation, semanticModel));
        var surroundingMemberAccess = invocation.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
        var nestedMemberAccess = invocation.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();
        if (nestedMemberAccess == null || surroundingMemberAccess == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Remove unnecessary {operation} call",
                x => RemoveMaterialization(context.Document, surroundingMemberAccess, nestedMemberAccess, root),
                UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id),
            diagnostic);
    }

    private Task<Document> RemoveMaterialization(Document document, MemberAccessExpressionSyntax surroundingMemberAccess, MemberAccessExpressionSyntax nestedMemberAccess, SyntaxNode root)
    {
        var newRoot = root.ReplaceNode(surroundingMemberAccess.Expression, nestedMemberAccess.Expression);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}