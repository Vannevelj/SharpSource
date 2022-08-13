using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

using SharpSource.Utilities;

namespace SharpSource
{
    [ExportCodeFixProvider(DiagnosticId.ThreadSleepInAsyncMethod + "CF", LanguageNames.CSharp), Shared]
    public class ThreadSleepInAsyncMethodCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ThreadSleepInAsyncMethodAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(CodeFixResources.ThreadSleepInAsyncMethodCodeFixTitle,
                    x => UseTaskDelay(context.Document, memberAccess, root, diagnostic, x),
                    ThreadSleepInAsyncMethodAnalyzer.Rule.Id),
                diagnostic);
        }

        private Task<Document> UseTaskDelay(Document document, InvocationExpressionSyntax invocation, SyntaxNode root, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var isAsync = bool.Parse(diagnostic.Properties["isAsync"]);
            if (!isAsync)
            {
                return Task.FromResult(document);
            }

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
            var awaitExpression = SyntaxFactory.AwaitExpression(newInvocation).WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(invocation, awaitExpression);

            var compilationUnit = (CompilationUnitSyntax)newRoot;
            newRoot = compilationUnit.AddUsingStatementIfMissing("System.Threading.Tasks");

            var newDocument = document.WithSyntaxRoot(newRoot);

            return Task.FromResult(newDocument);
        }
    }
}