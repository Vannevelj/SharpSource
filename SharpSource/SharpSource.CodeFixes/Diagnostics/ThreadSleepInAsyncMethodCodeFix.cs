using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ThreadSleepInAsyncMethodCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ThreadSleepInAsyncMethodAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var memberAccess = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

        var isAsync = bool.Parse(diagnostic.Properties["isAsync"]);
        if (!isAsync || memberAccess == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Use Task.Delay",
                x => UseTaskDelay(context.Document, memberAccess, root),
                ThreadSleepInAsyncMethodAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseTaskDelay(Document document, InvocationExpressionSyntax invocation, SyntaxNode root)
    {
        ExpressionSyntax newExpression;
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            newExpression = memberAccess
                .WithExpression(SyntaxFactory.IdentifierName("Task"))
                .WithName(SyntaxFactory.IdentifierName("Delay"));
        }
        else if (invocation.Expression is IdentifierNameSyntax)
        {
            newExpression = SyntaxFactory.ParseExpression("Task.Delay");
        }
        else
        {
            return Task.FromResult(document);
        }

        var newInvocation = invocation.WithExpression(newExpression);
        var awaitExpression = SyntaxFactory.AwaitExpression(newInvocation).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.AddImportsAnnotation, SymbolAnnotation.Create("System.Threading.Tasks.Task"));

        var newRoot = root.ReplaceNode(invocation, awaitExpression);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}