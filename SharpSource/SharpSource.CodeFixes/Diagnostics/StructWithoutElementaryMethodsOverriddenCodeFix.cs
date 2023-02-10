using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
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
public class StructWithoutElementaryMethodsOverriddenCodeFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds
        => ImmutableArray.Create(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetRequiredSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;


        diagnostic.Properties.TryGetValue("IsEqualsImplemented", out var implementEqualsString);
        diagnostic.Properties.TryGetValue("IsGetHashCodeImplemented", out var implementGetHashCodeString);
        diagnostic.Properties.TryGetValue("IsToStringImplemented", out var implementToStringString);

        var implementEquals = bool.Parse(implementEqualsString);
        var implementGetHashCode = bool.Parse(implementGetHashCodeString);
        var implementToString = bool.Parse(implementToStringString);

        var dict = new Dictionary<string, bool>
            {
                {"Equals()", implementEquals},
                {"GetHashCode()", implementGetHashCode},
                {"ToString()", implementToString}
            };

        var statement = root.FindNode(diagnosticSpan) as StructDeclarationSyntax;
        if (statement == default)
        {
            return;
        }

        context.RegisterCodeFix(CodeAction.Create(
            string.Format("Implement {0}", FormatMissingMembers(dict)),
                x => AddMissingMethodsAsync(context.Document, root, statement,
                        implementEquals, implementGetHashCode, implementToString),
                StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.Id), diagnostic);
    }

    private static Task<Document> AddMissingMethodsAsync(Document document, SyntaxNode root, StructDeclarationSyntax statement, bool implementEquals, bool implementGetHashCode, bool implementToString)
    {
        var newStatement = statement;

        if (!implementEquals)
        {
            var isNullable = document.Project.CompilationOptions?.NullableContextOptions is not NullableContextOptions.Disable;
            newStatement = newStatement.AddMembers(GetEqualsMethod(isNullable));
        }

        if (!implementGetHashCode)
        {
            newStatement = newStatement.AddMembers(GetGetHashCodeMethod());
        }

        if (!implementToString)
        {
            newStatement = newStatement.AddMembers(GetToStringMethod());
        }

        var newRoot = root.ReplaceNode(statement, newStatement);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
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

    private static MethodDeclarationSyntax GetToStringMethod()
    {
        var publicModifier = Token(SyntaxKind.PublicKeyword);
        var overrideModifier = Token(SyntaxKind.OverrideKeyword);
        var bodyStatement = ParseStatement("throw new System.NotImplementedException();").WithAdditionalAnnotations(Simplifier.Annotation);

        return MethodDeclaration(ParseTypeName("string"), "ToString")
                .AddModifiers(publicModifier, overrideModifier)
                .AddBodyStatements(bodyStatement)
                .WithAdditionalAnnotations(Formatter.Annotation);
    }

    private static string FormatMissingMembers(Dictionary<string, bool> members)
    {
        // if we get this far, there are at least 1 missing members
        var missingMemberCount = 0;
        foreach (var member in members)
        {
            if (!member.Value)
            {
                missingMemberCount++;
            }
        }

        var value = new StringBuilder();
        for (var i = 0; i < members.Count; i++)
        {
            if (members.ElementAt(i).Value)
            {
                continue;
            }

            if (missingMemberCount == 2 && value.Length > 0)
            {
                value.Append(" and ");
            }

            value.Append(members.ElementAt(i).Key);

            if (missingMemberCount == 3 && i == 0)
            {
                value.Append(", ");
            }
            else if (missingMemberCount == 3 && i == 1)
            {
                value.Append(", and ");
            }
        }

        return value.ToString();
    }
}