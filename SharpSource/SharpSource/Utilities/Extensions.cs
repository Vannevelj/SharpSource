using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace SharpSource.Utilities;

public static class Extensions
{
    public static bool InheritsFrom(this ITypeSymbol symbol, ITypeSymbol candidateBaseType)
    {
        if (symbol == null || candidateBaseType == null)
        {
            return false;
        }

        var baseType = symbol;
        while (baseType != null)
        {
            if (baseType.Equals(candidateBaseType, SymbolEqualityComparer.Default))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    // TODO: tests
    // NOTE: string.Format() vs Format() (current/external type)
    public static bool IsAnInvocationOf(this SyntaxNode invocation, Type type, string method, SemanticModel semanticModel)
    {
        var (invokedType, invokedMethod) = invocation.GetInvocation(semanticModel);

        return invokedType?.MetadataName == type.Name &&
               invokedMethod?.MetadataName == method;
    }

    public static (INamedTypeSymbol? invokedType, ISymbol? invokedMethod) GetInvocation(this SyntaxNode invocation, SemanticModel semanticModel)
    {
        var invokedExpression = invocation switch
        {
            ConditionalAccessExpressionSyntax conditionalAccessExpression => conditionalAccessExpression.WhenNotNull,
            PostfixUnaryExpressionSyntax postfixUnaryExpression when postfixUnaryExpression.IsKind(SyntaxKind.SuppressNullableWarningExpression) => postfixUnaryExpression.Operand,
            InvocationExpressionSyntax invocationExpression => invocationExpression,
            _ => invocation
        };

        var invokedMethod = semanticModel.GetSymbolInfo(invokedExpression);
        var invokedType = invokedMethod.Symbol?.ContainingType;

        return (invokedType, invokedMethod.Symbol);
    }

    /// <summary>
    /// Gets the innermost surrounding class, struct or interface declaration
    /// </summary>
    /// <param name="syntaxNode">The node to start from</param>
    /// <returns>The surrounding declaration node or null</returns>
    public static TypeDeclarationSyntax? GetEnclosingTypeNode(this SyntaxNode syntaxNode) =>
        syntaxNode.FirstAncestorOrSelfOfType(
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration) as TypeDeclarationSyntax;

    public static IEnumerable<T> OfType<T>(this IEnumerable<SyntaxNode> enumerable, SyntaxKind kind) where T : SyntaxNode
    {
        foreach (var node in enumerable)
        {
            if (node.IsKind(kind))
            {
                yield return (T)node;
            }
        }
    }

    public static bool IsAnyKind(this SyntaxNode node, params SyntaxKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (node.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool IsTaskType(this ISymbol type) => type.IsNonGenericTaskType() || type.IsGenericTaskType(out _);

    internal static bool IsNonGenericTaskType(this ISymbol type) => type is INamedTypeSymbol { Arity: 0, Name: "Task" or "ValueTask" } && type.IsDefinedInSystemAssembly();

    internal static bool IsGenericTaskType(this ISymbol type, out ITypeSymbol? wrappedType)
    {
        if (type is INamedTypeSymbol { Arity: 1, Name: "Task" or "ValueTask" } namedType && type.IsDefinedInSystemAssembly())
        {
            wrappedType = namedType.TypeArguments.Single();
            return true;
        }

        wrappedType = null;
        return false;
    }

    public static SyntaxNode? FirstAncestorOrSelfOfType(this SyntaxNode node, params SyntaxKind[] kinds)
    {
        var parent = node;
        while (parent != default)
        {
            if (parent.IsAnyKind(kinds))
            {
                return parent;
            }
            parent = parent.Parent;
        }

        return null;
    }

    public static bool IsDefinedInSystemAssembly(this ISymbol symbol)
        => symbol.ContainingAssembly.Name == "mscorlib" ||
            symbol.ContainingAssembly.Name.StartsWith("System.") ||
            symbol.ContainingAssembly.Name.StartsWith("Microsoft.") ||
            symbol.ContainingAssembly.Name == "netstandard";

    public static IEnumerable<AttributeSyntax> GetAttributesOfType(this SyntaxList<AttributeListSyntax> attributes, Type type, SemanticModel semanticModel) =>
        attributes.SelectMany(x => x.Attributes).Where(a =>
        {
            var symbol = semanticModel.GetSymbolInfo(a.Name).Symbol?.ContainingSymbol;
            return symbol?.MetadataName == type.Name && symbol.IsDefinedInSystemAssembly();
        });

    public static IMethodSymbol GetBaseDefinition(this IMethodSymbol method)
    {
        while (method.IsOverride && method.OverriddenMethod != default)
        {
            method = method.OverriddenMethod;
        }

        return method;
    }

    public static bool HasASubsequentInvocation(this ExpressionSyntax node)
    {
        // If the invocation is wrapped in a nullable access, i.e. s1?.ToLower(), then the first visit will be the ConditionalAccessExpressionSyntax
        // If we would return on the first invocation then we would exit as soon as we reach ToLower()
        // For that reason we explicitly track the number of actual invocations we traverse
        var visitedInvocations = 0;

        var current = node;
        while (current != default)
        {
            if (current is InvocationExpressionSyntax)
            {
                visitedInvocations++;
            }

            if (visitedInvocations > 1)
            {
                return true;
            }

            current = current switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression,
                ConditionalAccessExpressionSyntax conditional => conditional.WhenNotNull,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
                _ => default
            };
        }

        return false;
    }

    public static (string? Name, bool? IsNullable) GetCancellationTokenFromParameters(this IMethodSymbol? method)
    {
        if (method == default)
        {
            return default;
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } ctoken && ctoken.TypeArguments.Single().Name == "CancellationToken")
            {
                return (parameter.Name, true);
            }

            if (parameter.Type is INamedTypeSymbol { Name: "CancellationToken" })
            {
                return (parameter.Name, false);
            }
        }

        return default;
    }

    public static bool PassesThroughCancellationToken(this IInvocationOperation invocation, INamedTypeSymbol cancellationTokenSymbol)
        => invocation.Arguments.Any(argument => cancellationTokenSymbol.Equals(argument.Parameter?.Type, SymbolEqualityComparer.Default));

    public static IEnumerable<IOperation?> Ancestors(this IOperation? operation)
    {
        while (operation is not null)
        {
            operation = operation.Parent;
            yield return operation;
        }
    }
}