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

namespace SharpSource.Diagnostics
{
    [ExportCodeFixProvider(DiagnosticId.ExplicitEnumValues + "CF", LanguageNames.CSharp), Shared]
    public class ExplicitEnumValuesCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ExplicitEnumValuesAnalyzer.Rule.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var statement = root.FindNode(diagnosticSpan).DescendantNodesAndSelf().OfType<EnumDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(CodeFixResources.ExplicitEnumValuesCodeFixTitle,
                    x => SpecifyEnumValue(context.Document, root, statement), ExplicitEnumValuesAnalyzer.Rule.Id), diagnostic);
        }

        private Task<Document> SpecifyEnumValue(Document document, SyntaxNode root, EnumDeclarationSyntax declaration)
        {
            
        }
    }
}