using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StaticInitializerAccessedBeforeInitializationAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "A static field relies on the value of another static field which is defined in the same type. Static fields are initialized in order of appearance.";

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StaticInitializerAccessedBeforeInitialization,
        Title,
        "{0} accesses {1} but both are marked as static and {1} will not be initialized when it is used",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS045-StaticInitializerAccessedBeforeInitialization.md"
    );

    public static DiagnosticDescriptor RuleForPartials => new(
        DiagnosticId.StaticInitializerAccessedBeforeInitialization,
        Title,
        "{0} accesses {1} but both are marked as static and {1} might not be initialized when it is used",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS045-StaticInitializerAccessedBeforeInitialization.md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleForPartials);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        if (!fieldDeclaration.Modifiers.Contains(SyntaxKind.StaticKeyword))
        {
            return;
        }

        var fieldType = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
        if (fieldType is { TypeKind: TypeKind.Delegate })
        {
            return;
        }

        var declarators = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(d => d.Initializer is not null);
        foreach (var declarator in declarators)
        {
            var identifiersInAssignment = declarator.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var (rule, identifier) in GetSuspectIdentifiers(identifiersInAssignment, context.SemanticModel, fieldDeclaration, declarator))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), declarator.Identifier.ValueText, identifier.Identifier.ValueText));
            }
        }
    }

    private static IEnumerable<(DiagnosticDescriptor, IdentifierNameSyntax)> GetSuspectIdentifiers(IEnumerable<IdentifierNameSyntax> identifiers, SemanticModel semanticModel, FieldDeclarationSyntax fieldDeclaration, VariableDeclaratorSyntax variableDeclarator)
    {
        var enclosingType = fieldDeclaration.GetEnclosingTypeNode();
        if (enclosingType == default)
        {
            yield break;
        }

        var isPartial = enclosingType.Modifiers.Contains(SyntaxKind.PartialKeyword);
        var enclosingTypeSymbol = semanticModel.GetDeclaredSymbol(enclosingType);

        foreach (var identifier in identifiers)
        {
            var referencedSymbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (referencedSymbol?.ContainingType == default)
            {
                continue;
            }

            if (!referencedSymbol.ContainingType.Equals(enclosingTypeSymbol, SymbolEqualityComparer.Default))
            {
                continue;
            }

            if (!referencedSymbol.IsStatic)
            {
                continue;
            }

            if (referencedSymbol is IMethodSymbol)
            {
                continue;
            }

            var referencedIdentifierDeclaration = referencedSymbol.DeclaringSyntaxReferences[0];
            if (fieldDeclaration.SpanStart > referencedIdentifierDeclaration.Span.Start)
            {
                continue;
            }

            // Don't trigger for nameof() calls, they are resolved at compile time
            var invocationNode = identifier.FirstAncestorOrSelfOfType(SyntaxKind.InvocationExpression);
            if (invocationNode is InvocationExpressionSyntax invocation && invocation.IsNameofInvocation())
            {
                continue;
            }

            // We _can_ call static functions
            if (invocationNode == identifier.Parent)
            {
                continue;
            }

            var constantValue = semanticModel.GetConstantValue(identifier);
            if (constantValue.HasValue)
            {
                continue;
            }

            var symbolOfDeclaredField = semanticModel.GetDeclaredSymbol(variableDeclarator);
            if (referencedSymbol.Equals(symbolOfDeclaredField, SymbolEqualityComparer.Default))
            {
                continue;
            }

            var surroundingObjectCreation = identifier.FirstAncestorOrSelfOfType(SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression) as BaseObjectCreationExpressionSyntax;
            if (surroundingObjectCreation != default)
            {
                var createdSymbol = surroundingObjectCreation.GetCreatedType(semanticModel);
                if (referencedSymbol.Kind == SymbolKind.Method &&
                    createdSymbol is INamedTypeSymbol { Name: "Lazy", Arity: 1 } lazySymbol &&
                    lazySymbol.IsDefinedInSystemAssembly())
                {
                    continue;
                }
            }

            // If it is wrapped in a lambda then static fields will be initialised by the time the lambda runs
            if (identifier.FirstAncestorOrSelfOfType(SyntaxKind.ParenthesizedLambdaExpression) != default)
            {
                continue;
            }

            yield return isPartial ? (RuleForPartials, identifier) : (Rule, identifier);
        }
    }
}