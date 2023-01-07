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
public class AttributeMustSpecifyAttributeUsageCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(AttributeMustSpecifyAttributeUsageAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (root is not CompilationUnitSyntax compilation || classDeclaration == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create("Add [AttributeUsage]",
                x => AddAttributeUsage(context.Document, compilation, classDeclaration),
                AttributeMustSpecifyAttributeUsageAnalyzer.Rule.Id),
            diagnostic);
    }

    private static Task<Document> AddAttributeUsage(Document document, CompilationUnitSyntax root, ClassDeclarationSyntax classDeclaration)
    {
        // Generates [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        var arguments = new[]
        {
            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("AttributeTargets.All")),
            SyntaxFactory.AttributeArgument(SyntaxFactory.NameEquals("AllowMultiple"), null, SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))
        };
        var newAttribute =
            SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName("AttributeUsage"),
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(arguments)));

        var newClass = classDeclaration.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] { newAttribute.WithAdditionalAnnotations(Simplifier.AddImportsAnnotation, SymbolAnnotation.Create("System.AttributeUsageAttribute")) })));
        var newRoot = root.ReplaceNode(classDeclaration, newClass);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}