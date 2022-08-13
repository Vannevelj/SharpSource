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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource
{
    [ExportCodeFixProvider(DiagnosticId.SynchronousTaskWait + "CF", LanguageNames.CSharp), Shared]
    public class SynchronousTaskWaitCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(SynchronousTaskWaitAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var synchronousWaitMethod = root.FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<MemberAccessExpressionSyntax>()
                .SingleOrDefault(x => x.Name.Identifier.ValueText == "Wait");

            context.RegisterCodeFix(
                CodeAction.Create("Use await",
                    x => UseAwait(context.Document, synchronousWaitMethod, root, x),
                    SynchronousTaskWaitAnalyzer.Rule.Id),
                diagnostic);
        }

        private Task<Document> UseAwait(Document document, MemberAccessExpressionSyntax memberAccessExpression, SyntaxNode root, CancellationToken x)
        {
            if (memberAccessExpression == null)
            {
                return Task.FromResult(document);
            }

            var newExpression = AwaitExpression(memberAccessExpression.Expression);
            var originalInvocation = memberAccessExpression.FirstAncestorOrSelf<InvocationExpressionSyntax>();

            var newRoot = root.ReplaceNode(originalInvocation, newExpression);
            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}