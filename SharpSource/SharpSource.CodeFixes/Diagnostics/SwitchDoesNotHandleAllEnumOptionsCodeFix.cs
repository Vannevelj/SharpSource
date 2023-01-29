using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Simplification;
using SharpSource.Utilities;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SwitchDoesNotHandleAllEnumOptionsCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (semanticModel == default)
        {
            return;
        }

        var statement = root.FindNode(diagnosticSpan);
        var switchStatement = statement.Parent as SwitchStatementSyntax;
        if (switchStatement == default)
        {
            return;
        }

        var enumType = semanticModel.GetTypeInfo(switchStatement.Expression).Type as INamedTypeSymbol;
        var caseLabels = switchStatement.Sections.SelectMany(l => l.Labels)
                                    .OfType<CaseSwitchLabelSyntax>()
                                    .Select(l => l.Value);

        var missingLabels = GetMissingLabels(caseLabels, enumType);
        if (enumType is null || missingLabels is null || switchStatement is null)
        {
            return;
        }

        var notImplementedException = ThrowStatement(ParseExpression($" new System.NotImplementedException()")).WithAdditionalAnnotations(Simplifier.Annotation);
        var statements = List(new List<StatementSyntax> { notImplementedException });

        context.RegisterCodeFix(
            CodeAction.Create("Add cases",
                x => AddMissingCaseAsync(context.Document, enumType, missingLabels, (CompilationUnitSyntax)root, switchStatement, statements),
                SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.Id), diagnostic);
    }

    private static async Task<Document> AddMissingCaseAsync(Document document, INamedTypeSymbol enumType, IEnumerable<ISymbol> missingLabels, CompilationUnitSyntax root, SwitchStatementSyntax switchBlock, SyntaxList<StatementSyntax> sectionBody)
    {
        var allSections = new List<SwitchSectionSyntax>(switchBlock.Sections);

        foreach (var label in missingLabels)
        {
            var expression = ParseExpression($"{enumType?.ToDisplayString()}.{label.Name}").WithAdditionalAnnotations(Simplifier.Annotation);
            var caseLabel = CaseSwitchLabel(expression);
            var section = SwitchSection(List(new SwitchLabelSyntax[] { caseLabel }), sectionBody).WithAdditionalAnnotations(Formatter.Annotation);

            // ensure that the new cases are above the default case
            allSections.Insert(0, section);
        }

        var newSections = List(allSections);
        var newNode = switchBlock.WithSections(newSections).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

        var newRoot = root.ReplaceNode(switchBlock, newNode);
        var newDocument = await Simplifier.ReduceAsync(document.WithSyntaxRoot(newRoot)).ConfigureAwait(false);
        return newDocument;
    }

    private static IEnumerable<ISymbol> GetMissingLabels(IEnumerable<ExpressionSyntax> caseLabels, INamedTypeSymbol? enumType)
    {
        if (enumType == default)
        {
            return Enumerable.Empty<ISymbol>();
        }

        var membersToIgnore = new HashSet<string>();

        foreach (var label in caseLabels)
        {
            // these are the labels like `MyEnum.EnumMember`
            if (label is MemberAccessExpressionSyntax memberAccess)
            {
                membersToIgnore.Add(memberAccess.Name.Identifier.ValueText);
            }

            // these are the labels like `EnumMember` (such as when using `using static Namespace.MyEnum;`)
            else if (label is IdentifierNameSyntax identifier)
            {
                membersToIgnore.Add(identifier.Identifier.ValueText);
            }
        }

        // don't create members like ".ctor"
        return enumType.GetMembers().Where(member => !membersToIgnore.Contains(member.Name) && member.Name != WellKnownMemberNames.InstanceConstructorName);
    }
}