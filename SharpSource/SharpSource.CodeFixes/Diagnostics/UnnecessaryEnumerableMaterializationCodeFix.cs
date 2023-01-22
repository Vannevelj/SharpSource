using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class UnnecessaryEnumerableMaterializationCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var invocation = root.FindNode(diagnosticSpan).GetOuterParentOfType(SyntaxKind.ConditionalAccessExpression, SyntaxKind.InvocationExpression);
        var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

        var operation = diagnostic.Properties["operation"];
        if (operation == default || invocation == default || semanticModel == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                $"Remove unnecessary {operation} call",
                x => RemoveMaterialization(context.Document, invocation, root, semanticModel, operation),
                UnnecessaryEnumerableMaterializationAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> RemoveMaterialization(Document document, SyntaxNode invocation, SyntaxNode root, SemanticModel semanticModel, string invokedFunction)
    {
        var newInvocation = invocation.RemoveInvocation(typeof(Enumerable), invokedFunction, semanticModel);
        var newRoot = root.ReplaceNode(invocation, newInvocation);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}