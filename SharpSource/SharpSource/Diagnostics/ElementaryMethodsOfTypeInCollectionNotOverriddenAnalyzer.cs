using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ElementaryMethodsOfTypeInCollectionNotOverriddenAnalyzer : DiagnosticAnalyzer
{
    private static readonly (Type type, string method)[] SupportedLookups = new[] {
        (typeof(List<>), "Contains"),
        (typeof(HashSet<>), "Contains"),
        (typeof(ReadOnlyCollection<>), "Contains"),
        (typeof(Queue<>), "Contains"),
        (typeof(Stack<>), "Contains"),
        (typeof(Enumerable), "Contains"),
        (typeof(IEnumerable), "Contains"),
        (typeof(Dictionary<,>), "Contains"),
        (typeof(Dictionary<,>), "TryGetValue"),
        (typeof(Dictionary<,>), "ContainsKey"),
        (typeof(Dictionary<,>), "ContainsValue"),
        (typeof(Dictionary<,>), "Item") // indexer
    };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ElementaryMethodsOfTypeInCollectionNotOverridden,
        "Implement Equals() and GetHashcode() methods for a type used in a collection.",
        "Type {0} is used in a collection lookup but does not override Equals() and GetHashCode()",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS004-ElementaryMethodsOfTypeInCollectionNotOverridden.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression, SyntaxKind.ElementAccessExpression);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var argument = context.Node switch
        {
            InvocationExpressionSyntax invocation => invocation.ArgumentList?.Arguments.FirstOrDefault(),
            ElementAccessExpressionSyntax indexer => indexer.ArgumentList?.Arguments.FirstOrDefault(),
            _ => default
        };
        if (argument == default)
        {
            return;
        }

        if (context.Node is InvocationExpressionSyntax invocationExpression &&
            !SupportedLookups.Any(lookup => invocationExpression.IsAnInvocationOf(lookup.type, lookup.method, context.SemanticModel)))
        {
            return;
        }

        var invokedType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
        if (invokedType == null ||
            invokedType.TypeKind == TypeKind.Interface ||
            invokedType.TypeKind == TypeKind.TypeParameter ||
            invokedType.TypeKind == TypeKind.Enum ||
            invokedType.TypeKind == TypeKind.Array ||
            invokedType.IsDefinedInSystemAssembly())
        {
            return;
        }

        var implementsEquals = false;
        var implementsGetHashCode = false;
        foreach (var member in invokedType.GetMembers())
        {
            if (member.Name == WellKnownMemberNames.ObjectEquals)
            {
                implementsEquals = true;
            }

            if (member.Name == WellKnownMemberNames.ObjectGetHashCode)
            {
                implementsGetHashCode = true;
            }
        }

        if (!implementsEquals || !implementsGetHashCode)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, argument.GetLocation(), invokedType.Name));
        }
    }
}