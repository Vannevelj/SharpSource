using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GetHashCodeRefersToMutableMemberAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "GetHashCode() refers to mutable or static member";

    public static DiagnosticDescriptor FieldRule => new(
        DiagnosticId.GetHashCodeRefersToMutableMember,
        Title,
        "GetHashCode() refers to mutable field {0}",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS008-GetHashCodeRefersToMutableMember.md");

    public static DiagnosticDescriptor PropertyRule => new(
        DiagnosticId.GetHashCodeRefersToMutableMember,
        Title,
        "GetHashCode() refers to mutable property {0}",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS008-GetHashCodeRefersToMutableMember.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(FieldRule, PropertyRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var declaration = (MethodDeclarationSyntax)context.Node;
        if (declaration is not { Identifier.ValueText: "GetHashCode", ParameterList.Parameters.Count: 0 })
        {
            return;
        }

        var currentType = context.SemanticModel.GetDeclaredSymbol(declaration)?.ContainingSymbol;
        var nodes = declaration.DescendantNodes(descendIntoChildren: target => true);

        var identifierNameNodes = nodes.OfType<IdentifierNameSyntax>(SyntaxKind.IdentifierName);
        foreach (var node in identifierNameNodes)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
            if (symbol == null)
            {
                continue;
            }

            switch (symbol)
            {
                case IFieldSymbol fieldSymbol:
                    var fieldIsMutableOrStatic = FieldIsMutable(fieldSymbol);
                    if (fieldIsMutableOrStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(FieldRule, node.GetLocation(), symbol.Name));
                    }
                    break;
                case IPropertySymbol propertySymbol:
                    if (!propertySymbol.ContainingType.Equals(currentType, SymbolEqualityComparer.Default))
                    {
                        continue;
                    }

                    var propertyIsMutable = PropertyIsMutable((IPropertySymbol)symbol);
                    if (propertyIsMutable)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(PropertyRule, node.GetLocation(), symbol.Name));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private static bool FieldIsMutable(IFieldSymbol field)
    {
        if (field.IsConst)
        {
            return false;
        }

        if (field.IsReadOnly && ( field.Type.IsValueType || field.Type.SpecialType == SpecialType.System_String ) && !field.IsStatic)
        {
            return false;
        }

        return true;
    }

    private static bool PropertyIsMutable(IPropertySymbol property) => property is { SetMethod.IsInitOnly: false };
}