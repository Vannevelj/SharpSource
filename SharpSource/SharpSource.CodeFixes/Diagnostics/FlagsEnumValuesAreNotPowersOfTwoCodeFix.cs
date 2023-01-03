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
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FlagsEnumValuesAreNotPowersOfTwoCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FlagsEnumValuesAreNotPowersOfTwoAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var enumMember = root?.FindNode(diagnosticSpan)?.FirstAncestorOrSelfOfType(SyntaxKind.EnumMemberDeclaration) as EnumMemberDeclarationSyntax;
        var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);
        if (root == default || enumMember == default || semanticModel == default)
        {
            return;
        }

        var targetConstant = semanticModel.GetDeclaredSymbol(enumMember)?.ConstantValue;
        if (targetConstant == default)
        {
            return;
        }

        var enumDeclaration = enumMember.FirstAncestorOrSelfOfType(SyntaxKind.EnumDeclaration);
        if (enumDeclaration == default)
        {
            return;
        }

        var otherEnumMembers = enumDeclaration
            .DescendantNodes()
            .OfType<EnumMemberDeclarationSyntax>()
            .Where(em => em.EqualsValue is { Value: LiteralExpressionSyntax })
            .Except(new[] { enumMember })
            .ToList();

        // Make sure we don't recommend the same combination twice
        // We do want to show the reverse, i.e. both A|B and B|A
        HashSet<(object?, object?)> suggestedOptions = new();
        foreach (var otherEnumMember in otherEnumMembers)
        {
            foreach (var otherEnumMemberAgain in otherEnumMembers)
            {
                // We won't use the same member in both operands
                if (otherEnumMember.Equals(otherEnumMemberAgain))
                {
                    continue;
                }

                var firstEnumMemberConstantValue = semanticModel.GetDeclaredSymbol(otherEnumMember)?.ConstantValue;
                var secondEnumMemberConstantValue = semanticModel.GetDeclaredSymbol(otherEnumMemberAgain)?.ConstantValue;
                if (IsCompatible(targetConstant, firstEnumMemberConstantValue, secondEnumMemberConstantValue) &&
                    suggestedOptions.Add((firstEnumMemberConstantValue, secondEnumMemberConstantValue)))
                {
                    context.RegisterCodeFix(
                        CodeAction.Create("Use OR expression",
                            x => UseBitwiseExpression(
                                context.Document,
                                root,
                                enumMember,
                                otherEnumMember.Identifier,
                                otherEnumMemberAgain.Identifier),
                            FlagsEnumValuesAreNotPowersOfTwoAnalyzer.Rule.Id), diagnostic);
                }
            }
        }
    }

    private static Task<Document> UseBitwiseExpression(Document document, SyntaxNode root, EnumMemberDeclarationSyntax enumMember, SyntaxToken firstIdentifier, SyntaxToken secondIdentifier)
    {
        var newValue = SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, SyntaxFactory.IdentifierName(firstIdentifier.WithoutTrivia()), SyntaxFactory.IdentifierName(secondIdentifier.WithoutTrivia()));
        var newMember = enumMember.WithEqualsValue(SyntaxFactory.EqualsValueClause(newValue)).WithAdditionalAnnotations(Formatter.Annotation);
        var newRoot = root.ReplaceNode(enumMember, newMember);
        var newDocument = document.WithSyntaxRoot(newRoot);
        return Task.FromResult(newDocument);
    }

    private static bool IsCompatible(object? target, object? first, object? second) =>
        (target, first, second) switch
        {
            (int t, int f, int s) => ( f | s ) == t,
            (uint t, uint f, uint s) => ( f | s ) == t,
            (byte t, byte f, byte s) => ( f | s ) == t,
            (sbyte t, sbyte f, sbyte s) => ( f | s ) == t,
            (long t, long f, long s) => ( f | s ) == t,
            (ulong t, ulong f, ulong s) => ( f | s ) == t,
            (short t, short f, short s) => ( f | s ) == t,
            (ushort t, ushort f, ushort s) => ( f | s ) == t,
            _ => false
        };
}