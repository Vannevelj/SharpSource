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
public class DisposeAsyncDisposableCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DisposeAsyncDisposableAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var statement = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        StatementSyntax? newStatement = statement switch
        {
            LocalDeclarationStatementSyntax local => local.WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword)),
            UsingStatementSyntax @using => @using.WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword)),
            _ => default
        };

        if (newStatement is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Dispose asynchronously",
                x => Modify(context.Document, root, statement, newStatement), DateTimeNowAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> Modify(Document document, SyntaxNode root, SyntaxNode statement, StatementSyntax newStatement)
    {
        var newRoot = root.ReplaceNode(statement, newStatement);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}