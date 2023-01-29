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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

using SharpSource.Utilities;

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
                                    .Select(l => l.Value)
                                    .ToList();

        var missingLabels = GetMissingLabels(caseLabels, enumType);

        // use simplified form if there are any in simplified form or if there are not any labels at all
        var hasSimplifiedLabel = caseLabels.OfType<IdentifierNameSyntax>().Any();
        var useSimplifiedForm = ( hasSimplifiedLabel || !caseLabels.OfType<MemberAccessExpressionSyntax>().Any() ) && caseLabels.Any();

        if (enumType is null || missingLabels is null || switchStatement is null)
        {
            return;
        }

        var qualifier = GetQualifierForException((CompilationUnitSyntax)root);

        var notImplementedException = ThrowStatement(ParseExpression($" new {qualifier}NotImplementedException()")).WithAdditionalAnnotations(Simplifier.Annotation);
        var statements = List(new List<StatementSyntax> { notImplementedException });

        context.RegisterCodeFix(
            CodeAction.Create("Add cases",
                x => AddMissingCaseAsync(context.Document, enumType, missingLabels, useSimplifiedForm, (CompilationUnitSyntax)root, switchStatement, statements),
                SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.Id), diagnostic);
    }

    private static async Task<Document> AddMissingCaseAsync(Document document, INamedTypeSymbol enumType, IEnumerable<string> missingLabels, bool useSimplifiedForm, CompilationUnitSyntax root, SwitchStatementSyntax switchBlock, SyntaxList<StatementSyntax> sectionBody)
    {
        var allSections = new List<SwitchSectionSyntax>(switchBlock.Sections);

        foreach (var label in missingLabels)
        {
            var expression = ParseExpression($"{enumType?.ToDisplayString()}.{label}").WithAdditionalAnnotations(Simplifier.Annotation);
            var caseLabel = CaseSwitchLabel(expression);
            var section = SwitchSection(List(new SwitchLabelSyntax[] { caseLabel }), sectionBody).WithAdditionalAnnotations(Formatter.Annotation);

            // ensure that the new cases are above the default case
            allSections.Insert(0, section);
        }

        var newSections = List(allSections);
        var newNode = useSimplifiedForm
            ? switchBlock.WithSections(newSections).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
            : switchBlock.WithSections(newSections).WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(switchBlock, newNode);
        var newDocument = await Simplifier.ReduceAsync(document.WithSyntaxRoot(newRoot)).ConfigureAwait(false);
        return newDocument;
    }

    private static IEnumerable<string> GetMissingLabels(List<ExpressionSyntax> caseLabels, INamedTypeSymbol? enumType)
    {
        if (enumType == default)
        {
            return Enumerable.Empty<string>();
        }

        // these are the labels like `MyEnum.EnumMember`
        var labels = caseLabels
            .OfType<MemberAccessExpressionSyntax>()
            .Select(l => l.Name.Identifier.ValueText)
            .ToList();

        // these are the labels like `EnumMember` (such as when using `using static Namespace.MyEnum;`)
        labels.AddRange(caseLabels.OfType<IdentifierNameSyntax>().Select(l => l.Identifier.ValueText));

        // don't create members like ".ctor"
        return enumType.GetMembers().Where(member => !labels.Contains(member.Name) && member.Name != WellKnownMemberNames.InstanceConstructorName).Select(member => member.Name);
    }

    private string GetQualifierForException(CompilationUnitSyntax root)
    {
        var qualifier = "System.";
        var usingSystemDirective =
            root.Usings.Where(u => u.Name is IdentifierNameSyntax)
                .FirstOrDefault(u => ( (IdentifierNameSyntax)u.Name ).Identifier.ValueText == nameof(System));

        if (usingSystemDirective != null)
        {
            qualifier = usingSystemDirective.Alias == null
                ? string.Empty
                : usingSystemDirective.Alias.Name.Identifier.ValueText + ".";
        }
        return qualifier;
    }
}