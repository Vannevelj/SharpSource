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
public class LinqTraversalBeforeFilterCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(LinqTraversalBeforeFilterAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var traversalInvocation = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as InvocationExpressionSyntax;

        if (traversalInvocation?.Expression is not MemberAccessExpressionSyntax traversalMemberAccess)
        {
            return;
        }

        // Find the Where() invocation that wraps this traversal
        var whereInvocation = traversalInvocation.FirstAncestorOrSelf<InvocationExpressionSyntax>(
            inv => inv != traversalInvocation && inv.Expression is MemberAccessExpressionSyntax ma && ma.Name.Identifier.Text == "Where");

        if (whereInvocation?.Expression is not MemberAccessExpressionSyntax whereMemberAccess)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Move Where() before traversal",
                x => SwapTraversalAndFilter(context.Document, root, traversalInvocation, traversalMemberAccess, whereInvocation, whereMemberAccess),
                LinqTraversalBeforeFilterAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> SwapTraversalAndFilter(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax traversalInvocation,
        MemberAccessExpressionSyntax traversalMemberAccess,
        InvocationExpressionSyntax whereInvocation,
        MemberAccessExpressionSyntax whereMemberAccess)
    {
        // Original: source.Traversal(args).Where(predicate)
        // Target:   source.Where(predicate).Traversal(args)

        // Get the source expression (what the traversal is called on)
        var sourceExpression = traversalMemberAccess.Expression;

        // Get the Where() arguments (the predicate)
        var whereArguments = whereInvocation.ArgumentList;

        // Get the traversal method name and arguments
        var traversalName = traversalMemberAccess.Name;
        var traversalArguments = traversalInvocation.ArgumentList;

        // Build: source.Where(predicate)
        var newWhereCall = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                sourceExpression,
                SyntaxFactory.IdentifierName("Where")),
            whereArguments);

        // Build: source.Where(predicate).Traversal(args)
        var newTraversalCall = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                newWhereCall,
                traversalName),
            traversalArguments);

        // Replace the entire Where() invocation with our new expression
        var newRoot = root.ReplaceNode(whereInvocation, newTraversalCall.WithTriviaFrom(whereInvocation));
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
