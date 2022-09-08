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
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StaticInitializerAccessedBeforeInitialization,
        "A static field relies on the value of another static field which is defined in the same type. Static fields are initialized in order of appearance.",
        "{0} accesses {1} but both are marked as static and {1} will not be initialized when it is used",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true
    );

    public static DiagnosticDescriptor RuleForPartials => new(
        DiagnosticId.StaticInitializerAccessedBeforeInitialization,
        "A static field relies on the value of another static field which is defined in the same type. Static fields are initialized in order of appearance.",
        "{0} accesses {1} but both are marked as static and {1} might not be initialized when it is used",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        var declarators = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
        var assignments = declarators.Where(d => d.Initializer is not null);

        foreach (var assignment in assignments)
        {
            var identifiersInAssignment = assignment.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var (rule, identifier) in GetSuspectIdentifiers(identifiersInAssignment, context.SemanticModel, fieldDeclaration))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), assignment.Identifier.ValueText, identifier.Identifier.ValueText));
            }
        }
    }

    private static IEnumerable<(DiagnosticDescriptor, IdentifierNameSyntax)> GetSuspectIdentifiers(IEnumerable<IdentifierNameSyntax> identifiers, SemanticModel semanticModel, FieldDeclarationSyntax fieldDeclaration)
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

            if (identifier.SyntaxTree != fieldDeclaration.SyntaxTree)
            {
                continue;
            }

            var referencedIdentifierDeclaration = referencedSymbol.DeclaringSyntaxReferences[0];
            if (fieldDeclaration.SpanStart > referencedIdentifierDeclaration.Span.Start)
            {
                continue;
            }


            yield return isPartial ? (RuleForPartials, identifier) : (Rule, identifier);
        }
    }
}