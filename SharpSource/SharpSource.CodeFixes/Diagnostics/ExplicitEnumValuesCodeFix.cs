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
using SharpSource.Utilities;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

            var statement = root.FindNode(diagnosticSpan).DescendantNodesAndSelf().OfType<EnumMemberDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(CodeFixResources.ExplicitEnumValuesCodeFixTitle,
                    x => SpecifyEnumValue(context.Document, root, statement, context.CancellationToken), ExplicitEnumValuesAnalyzer.Rule.Id), diagnostic);
        }

        private async Task<Document> SpecifyEnumValue(Document document, SyntaxNode root, EnumMemberDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync();

            var constantValue = semanticModel.GetConstantValue(declaration, cancellationToken);
            if (!constantValue.HasValue)
            {
                return document;
            }

            var newEqualsClause = EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal((int) constantValue.Value)));
            var newDeclaration = declaration.WithEqualsValue(newEqualsClause);
            var newDocument = root.ReplaceNode(declaration, newDeclaration);
            return document.WithSyntaxRoot(newDocument);
        }
    }
}