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

        var invocation = (InvocationExpressionSyntax)statement;
        if (invocation.Expression is not MemberAccessExpressionSyntax)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Is IsEmpty",
                x => UseIsEmpty(context.Document, root, invocation), ConcurrentDictionaryEmptyCheckAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> UseIsEmpty(Document document, SyntaxNode root, InvocationExpressionSyntax statement)
    {
        var memberAccessExpression = (MemberAccessExpressionSyntax)statement.Expression;
        var newExpression = memberAccessExpression.WithName(SyntaxFactory.IdentifierName("IsEmpty"));
        var newRoot = root.ReplaceNode(statement, newExpression);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}