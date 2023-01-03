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
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ParameterAssignedInConstructorCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.ParameterAssignedInConstructor);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var assignmentExpression = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().First();
        if (assignmentExpression == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Swap assignment",
                x => SwapAssignment(context.Document, root, assignmentExpression), DiagnosticId.ParameterAssignedInConstructor), diagnostic);
    }

    private static Task<Document> SwapAssignment(Document document, SyntaxNode root, AssignmentExpressionSyntax assignmentExpression)
    {
        var newAssignment = SyntaxFactory.AssignmentExpression(assignmentExpression.Kind(), assignmentExpression.Right, assignmentExpression.Left);
        var newRoot = root.ReplaceNode(assignmentExpression, newAssignment.WithAdditionalAnnotations(Formatter.Annotation));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}