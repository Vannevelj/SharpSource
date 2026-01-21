using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ImmutableCollectionCreatedIncorrectlyCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ImmutableCollectionCreatedIncorrectlyAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var objectCreation = root.FindNode(diagnosticSpan).DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
        if (objectCreation is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use ImmutableArray.Create()",
                x => UseCreateMethod(context.Document, root, objectCreation),
                ImmutableCollectionCreatedIncorrectlyAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> UseCreateMethod(Document document, SyntaxNode root, ObjectCreationExpressionSyntax objectCreation)
    {
        var typeArguments = objectCreation.Type switch
        {
            GenericNameSyntax genericName => genericName.TypeArgumentList,
            _ => null
        };

        if (typeArguments is null)
        {
            return Task.FromResult(document);
        }

        var memberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("ImmutableArray"),
            SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("Create"),
                typeArguments));

        var argumentList = objectCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
        
        var invocation = SyntaxFactory.InvocationExpression(memberAccess, argumentList)
            .WithTriviaFrom(objectCreation);

        var newRoot = root.ReplaceNode(objectCreation, invocation);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}