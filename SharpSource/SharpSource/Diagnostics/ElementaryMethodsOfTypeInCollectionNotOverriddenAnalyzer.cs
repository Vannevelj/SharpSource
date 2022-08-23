using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ElementaryMethodsOfTypeInCollectionNotOverriddenAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Implement Equals() and GetHashCode() methods of type {0} for use in a collection.";
    private static readonly string Title = "Implement Equals() and GetHashcode() methods for a type used in a collection.";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.ElementaryMethodsOfTypeInCollectionNotOverridden, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel.GetTypeInfo(context.Node).Type is not INamedTypeSymbol objectTypeInfo)
        {
            return;
        }

        var ienumerableIsImplemented = objectTypeInfo.ImplementsInterface(typeof(IEnumerable)) ||
                                       objectTypeInfo.ImplementsInterface(typeof(IEnumerable<>));

        if (!ienumerableIsImplemented)
        {
            return;
        }

        var node = (ObjectCreationExpressionSyntax)context.Node;
        if (node.Type is not GenericNameSyntax objectType)
        {
            return;
        }

        foreach (var genericType in objectType.TypeArgumentList.Arguments)
        {
            if (genericType == null)
            {
                return;
            }

            var genericTypeInfo = context.SemanticModel.GetTypeInfo(genericType).Type;
            if (genericTypeInfo == null ||
                genericTypeInfo.TypeKind == TypeKind.Interface ||
                genericTypeInfo.TypeKind == TypeKind.TypeParameter ||
                genericTypeInfo.TypeKind == TypeKind.Enum ||
                genericTypeInfo.TypeKind == TypeKind.Array)
            {
                return;
            }

            var implementsEquals = false;
            var implementsGetHashCode = false;
            foreach (var member in genericTypeInfo.GetMembers())
            {
                if (member.Name == nameof(Equals))
                {
                    implementsEquals = true;
                }

                if (member.Name == nameof(GetHashCode))
                {
                    implementsGetHashCode = true;
                }
            }

            if (!implementsEquals || !implementsGetHashCode)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, genericType.GetLocation(), genericTypeInfo.Name));
            }
        }
    }
}