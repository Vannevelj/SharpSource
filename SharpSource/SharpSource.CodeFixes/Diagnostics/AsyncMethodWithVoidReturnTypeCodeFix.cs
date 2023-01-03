using System;
using System.Collections.Immutable;
using System.Composition;
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
public class AsyncMethodWithVoidReturnTypeCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(AsyncMethodWithVoidReturnTypeAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var methodDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelfOfType(SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);

        if (methodDeclaration == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Use Task as return type",
                x => ChangeReturnTypeAsync(context.Document, methodDeclaration, root),
                AsyncMethodWithVoidReturnTypeAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> ChangeReturnTypeAsync(Document document, SyntaxNode methodDeclaration, SyntaxNode root)
    {
        SyntaxNode newMethod = methodDeclaration switch
        {
            MethodDeclarationSyntax method => method.WithReturnType(SyntaxFactory.ParseTypeName("Task").WithAdditionalAnnotations(Formatter.Annotation)),
            LocalFunctionStatementSyntax local => local.WithReturnType(SyntaxFactory.ParseTypeName("Task").WithAdditionalAnnotations(Formatter.Annotation)),
            _ => throw new NotSupportedException($"Unexpected node: {methodDeclaration.GetType().Name}")
        };

        var newRoot = root.ReplaceNode(methodDeclaration, newMethod);
        var compilation = (CompilationUnitSyntax)newRoot;
        newRoot = compilation.AddUsingStatementIfMissing("System.Threading.Tasks");

        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }
}