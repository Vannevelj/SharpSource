using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class EqualsAndGetHashcodeNotImplementedTogetherCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EqualsAndGetHashcodeNotImplementedTogetherAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var statement = root.FindNode(diagnosticSpan);

        if (bool.Parse(diagnostic.Properties["IsEqualsImplemented"]))
        {
            context.RegisterCodeFix(
                CodeAction.Create("Implement GetHashCode()",
                    x => ImplementGetHashCodeAsync(context.Document, root, statement),
                    EqualsAndGetHashcodeNotImplementedTogetherAnalyzer.Rule.Id),
                diagnostic);
        }
        else
        {
            context.RegisterCodeFix(
                CodeAction.Create("Implement Equals()",
                    x => ImplementEqualsAsync(context.Document, root, statement),
                    EqualsAndGetHashcodeNotImplementedTogetherAnalyzer.Rule.Id),
                diagnostic);
        }
    }

    private static async Task<Document> ImplementEqualsAsync(Document document, SyntaxNode root, SyntaxNode statement)
    {
        var classDeclaration = (ClassDeclarationSyntax)statement;

        var isNullable = document.Project.CompilationOptions?.NullableContextOptions is not NullableContextOptions.Disable;
        var newRoot = root.ReplaceNode(classDeclaration, classDeclaration.AddMembers(GetEqualsMethod(isNullable)));
        var newDocument = await Simplifier.ReduceAsync(document.WithSyntaxRoot(newRoot)).ConfigureAwait(false);
        return newDocument;
    }

    private static async Task<Document> ImplementGetHashCodeAsync(Document document, SyntaxNode root, SyntaxNode statement)
    {
        var classDeclaration = (ClassDeclarationSyntax)statement;

        var newRoot = root.ReplaceNode(classDeclaration, classDeclaration.AddMembers(GetGetHashCodeMethod()));
        var newDocument = await Simplifier.ReduceAsync(document.WithSyntaxRoot(newRoot)).ConfigureAwait(false);
        return newDocument;
    }

    private static MethodDeclarationSyntax GetEqualsMethod(bool isNullable)
    {
        var publicModifier = Token(SyntaxKind.PublicKeyword);
        var overrideModifier = Token(SyntaxKind.OverrideKeyword);
        var bodyStatement = ParseStatement("throw new System.NotImplementedException();").WithAdditionalAnnotations(Simplifier.Annotation);
        var parameter = Parameter(Identifier("obj")).WithType(ParseTypeName(isNullable ? "object?" : "object"));

        return MethodDeclaration(ParseTypeName("bool"), "Equals")
                .AddModifiers(publicModifier, overrideModifier)
                .AddBodyStatements(bodyStatement)
                .AddParameterListParameters(parameter)
                .WithAdditionalAnnotations(Formatter.Annotation);
    }

    private static MethodDeclarationSyntax GetGetHashCodeMethod()
    {
        var publicModifier = Token(SyntaxKind.PublicKeyword);
        var overrideModifier = Token(SyntaxKind.OverrideKeyword);
        var bodyStatement = ParseStatement("throw new System.NotImplementedException();").WithAdditionalAnnotations(Simplifier.Annotation);

        return MethodDeclaration(ParseTypeName("int"), "GetHashCode")
                .AddModifiers(publicModifier, overrideModifier)
                .AddBodyStatements(bodyStatement)
                .WithAdditionalAnnotations(Formatter.Annotation);
    }
}