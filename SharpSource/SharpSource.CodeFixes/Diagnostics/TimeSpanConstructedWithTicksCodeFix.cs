using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
public class TimeSpanConstructedWithTicksCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(TimeSpanConstructedWithTicksAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var objectCreation = root.FindNode(diagnosticSpan).DescendantNodesAndSelf().OfType<BaseObjectCreationExpressionSyntax>().FirstOrDefault();
        if (objectCreation is null)
        {
            return;
        }

        var argument = objectCreation.ArgumentList?.Arguments.FirstOrDefault();
        if (argument is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use TimeSpan.FromTicks()",
                x => UseFromTicks(context.Document, root, objectCreation, argument),
                TimeSpanConstructedWithTicksAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseFromTicks(Document document, SyntaxNode root, BaseObjectCreationExpressionSyntax objectCreation, ArgumentSyntax argument)
    {
        var fromTicks = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression($"System.TimeSpan.FromTicks({argument})");
        var simplified = fromTicks.WithExpression(fromTicks.Expression.WithAdditionalAnnotations(Simplifier.Annotation));

        var newRoot = root.ReplaceNode(objectCreation, simplified.WithTriviaFrom(objectCreation));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
