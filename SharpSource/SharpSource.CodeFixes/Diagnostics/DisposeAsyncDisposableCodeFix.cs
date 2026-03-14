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
        diagnostic.Properties.TryGetValue(DisposeAsyncDisposableAnalyzer.RewrittenTypePropertyName, out var rewrittenTypeName);

        var newStatement = statement switch
        {
            LocalDeclarationStatementSyntax local => RewriteLocalDeclaration(local, rewrittenTypeName),
            UsingStatementSyntax @using => RewriteUsingStatement(@using, rewrittenTypeName),
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
        var newRoot = root.ReplaceNode(statement, newStatement.WithTriviaFrom(statement));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private static StatementSyntax RewriteLocalDeclaration(LocalDeclarationStatementSyntax local, string? rewrittenTypeName)
        => local
            .WithoutTrivia()
            .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
            .WithDeclaration(RewriteDeclaration(local.Declaration, rewrittenTypeName));

    private static StatementSyntax RewriteUsingStatement(UsingStatementSyntax @using, string? rewrittenTypeName)
        => @using
            .WithoutTrivia()
            .WithAwaitKeyword(SyntaxFactory.Token(SyntaxKind.AwaitKeyword))
            .WithDeclaration(@using.Declaration is null ? null : RewriteDeclaration(@using.Declaration, rewrittenTypeName));

    private static VariableDeclarationSyntax RewriteDeclaration(VariableDeclarationSyntax declaration, string? rewrittenTypeName)
    {
        var rewrittenType = GetRewrittenTypeSyntax(declaration, rewrittenTypeName);
        return rewrittenType is null ? declaration : declaration.WithType(rewrittenType);
    }

    private static TypeSyntax? GetRewrittenTypeSyntax(VariableDeclarationSyntax declaration, string? rewrittenTypeName)
    {
        if (declaration.Type.IsVar || declaration.Variables.Count != 1 || string.IsNullOrWhiteSpace(rewrittenTypeName))
        {
            return default;
        }

        return SyntaxFactory.ParseTypeName(rewrittenTypeName!)
            .WithTrailingTrivia(SyntaxFactory.Space)
            .WithAdditionalAnnotations(Simplifier.Annotation);
    }
}