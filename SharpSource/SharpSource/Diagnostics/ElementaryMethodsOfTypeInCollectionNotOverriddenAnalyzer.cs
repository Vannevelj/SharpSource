using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ElementaryMethodsOfTypeInCollectionNotOverriddenAnalyzer : DiagnosticAnalyzer
{
    private static readonly (Type type, string method)[] SupportedLookups = new[] {
        (typeof(List<>), "Contains"),
        (typeof(HashSet<>), "Contains"),
        (typeof(HashSet<>), "Add"),
        (typeof(ReadOnlyCollection<>), "Contains"),
        (typeof(Queue<>), "Contains"),
        (typeof(Stack<>), "Contains"),
        (typeof(Enumerable), "Contains"),
        (typeof(IEnumerable), "Contains"),
        (typeof(Enumerable), "Distinct"),
        (typeof(IEnumerable), "Distinct"),
        (typeof(Enumerable), "ToHashSet"),
        (typeof(IEnumerable), "ToHashSet"),
        (typeof(Dictionary<,>), "Contains"),
        (typeof(Dictionary<,>), "Add"),
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var lookupWithMethodSymbols =
                from lookup in SupportedLookups
                let typeSymbol = compilationContext.Compilation.GetTypeByMetadataName(lookup.type.FullName)
                let methodSymbol = typeSymbol?.GetMembers(lookup.method).OfType<IMethodSymbol>().FirstOrDefault()
                where typeSymbol != null && methodSymbol != null
                select (typeSymbol, methodSymbol);

            // The dictionary indexer is a reference to the "Item" property
            compilationContext.RegisterOperationAction(context => Analyze(context, lookupWithMethodSymbols.ToArray()), OperationKind.Invocation, OperationKind.PropertyReference);
        });
    }

    private static void Analyze(OperationAnalysisContext context, (INamedTypeSymbol typeSymbol, IMethodSymbol methodSymbol)[] supportedLookups)
    {
        var argument = context.Operation switch
        {
            // Static methods (which includes extension methods) are methods where the instance is passed in as the first argument
            IInvocationOperation invocationOperation => invocationOperation.TargetMethod.IsStatic
                                                            ? invocationOperation.Arguments.Skip(1).FirstOrDefault()
                                                            : invocationOperation.Arguments.FirstOrDefault(),
            IPropertyReferenceOperation propertyReference => propertyReference.Arguments.FirstOrDefault(),
            _ => default
        };

        // Invocation of .Distinct() or .ToHashSet()
        var extensionMethodInvocationWithoutArguments = GetExtensionMethodInvocationWithoutArgumentsOrDefault(context.Operation);

        if (argument == null && extensionMethodInvocationWithoutArguments == null)
        {
            return;
        }

        var argumentType = argument?.Parameter?.Type ?? extensionMethodInvocationWithoutArguments?.TargetMethod.TypeArguments.FirstOrDefault();

        if (argumentType == null)
        {
            return;
        }

        if (argumentType.TypeKind is TypeKind.TypeParameter
                                  or TypeKind.Interface
                                  or TypeKind.Enum
                                  or TypeKind.Array
            || argumentType.IsDefinedInSystemAssembly())
        {
            return;
        }

        if (context.Operation is IInvocationOperation invocation &&
            !supportedLookups.Any(lookup =>
                lookup.typeSymbol.Equals(invocation.TargetMethod.OriginalDefinition.ContainingType, SymbolEqualityComparer.Default) &&
                lookup.methodSymbol.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var implementsEquals = false;
        var implementsGetHashCode = false;
        var currentTypeInHierarchy = argumentType;
        do
        {
            foreach (var member in currentTypeInHierarchy.GetMembers())
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
            currentTypeInHierarchy = currentTypeInHierarchy.BaseType;
        } while (currentTypeInHierarchy is not ( null or { SpecialType: SpecialType.System_Object or SpecialType.System_ValueType } ));

        if (!implementsEquals || !implementsGetHashCode)
        {
            var syntaxNode = argument?.Syntax ?? extensionMethodInvocationWithoutArguments!.Syntax;
            context.ReportDiagnostic(Diagnostic.Create(Rule, syntaxNode.GetLocation(), argumentType.Name));
        }
    }

    private static IInvocationOperation? GetExtensionMethodInvocationWithoutArgumentsOrDefault(IOperation operation)
        => operation is IInvocationOperation
        {
            TargetMethod.IsExtensionMethod: true,
            TargetMethod.IsGenericMethod: true,
            TargetMethod.TypeArguments.Length: 1,
            Arguments.Length: 1 // `this` in extension method
        } extensionInvocation
            ? extensionInvocation
            : default;
}