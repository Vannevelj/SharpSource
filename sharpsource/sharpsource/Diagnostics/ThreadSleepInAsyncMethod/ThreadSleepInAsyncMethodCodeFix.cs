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

namespace SharpSource.Diagnostics.ThreadSleepInAsyncMethod
{
    [ExportCodeFixProvider(DiagnosticId.ThreadSleepInAsyncMethod + "CF", LanguageNames.CSharp), Shared]
    public class ThreadSleepInAsyncMethodCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(ThreadSleepInAsyncMethodAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var memberAccess = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(Resources.ThreadSleepInAsyncMethodCodeFixTitle,
                    x => UseTaskDelay(context.Document, memberAccess, root, x),
                    ThreadSleepInAsyncMethodAnalyzer.Rule.Id),
                diagnostic);
        }

        private Task<Document> UseTaskDelay(Document document, MemberAccessExpressionSyntax memberAccess, SyntaxNode root, CancellationToken cancellationToken)
        {
            var newMemberAccess = memberAccess
                    .WithExpression(SyntaxFactory.IdentifierName("Task"))
                    .WithName(SyntaxFactory.IdentifierName("Delay"));

            var invocation = memberAccess.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return Task.FromResult(document);
            }

            var newInvocation = invocation.WithExpression(newMemberAccess);

            var awaitExpression = SyntaxFactory.AwaitExpression(newInvocation).WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(invocation, awaitExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return Task.FromResult(newDocument);
        }
    }
}
