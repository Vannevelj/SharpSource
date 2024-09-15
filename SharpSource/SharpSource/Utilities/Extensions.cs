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

            foreach (var @interface in baseType.AllInterfaces)
            {
                if (@interface.Equals(candidateBaseType, SymbolEqualityComparer.Default))
                {
                    return true;
                }
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
        => symbol.ContainingAssembly is not null && (
            symbol.ContainingAssembly.Name == "mscorlib" ||
            symbol.ContainingAssembly.Name.StartsWith("System.") ||
            symbol.ContainingAssembly.Name.StartsWith("Microsoft.") ||
            symbol.ContainingAssembly.Name == "netstandard" );

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

            if (parameter is { Type: INamedTypeSymbol { Name: "CancellationToken" }, IsOptional: true })
            {
                return (parameter.Name, true);
            }

            if (parameter is { Type: INamedTypeSymbol { Name: "CancellationToken" } })
            {
                return (parameter.Name, false);
            }
        }

        return default;
    }

    public static bool PassesThroughCancellationToken(this IInvocationOperation invocation, INamedTypeSymbol? cancellationTokenSymbol)
        => cancellationTokenSymbol != default && invocation.Arguments.Any(argument => cancellationTokenSymbol.Equals(argument.Parameter?.Type, SymbolEqualityComparer.Default));

    public static IEnumerable<IOperation?> Ancestors(this IOperation? operation)
    {
        while (operation is not null)
        {
            operation = operation.Parent;
            yield return operation;
        }
    }

    public static bool IsInterfaceImplementation(this IMethodSymbol method)
        => method.ContainingType
                 .AllInterfaces
                 .SelectMany(@interface => @interface.GetMembers().OfType<IMethodSymbol>())
                 .Any(interfaceMethod => method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod)?.Equals(method, SymbolEqualityComparer.Default) == true);

    public static IEnumerable<ISymbol> GetAllMembers(this INamespaceOrTypeSymbol symbol, params string[] members)
    {
        foreach (var member in members)
        {
            var symbolMembers = symbol.GetMembers(member);
            foreach (var symbolMember in symbolMembers)
            {
                yield return symbolMember;
            }
        }
    }

    public static IInvocationOperation? GetPrecedingInvocation(this IInvocationOperation invocation)
    {
        if (invocation.Instance is IInvocationOperation previousInvocation)
        {
            return previousInvocation;
        }

        if (invocation.Arguments is { Length: > 0 } && invocation.Arguments[0].Value is IConversionOperation { Operand: IInvocationOperation precedingInvocation })
        {
            return precedingInvocation;
        }

        return default;
    }

    public static ITypeSymbol? GetTypeOfInstanceInInvocation(this IInvocationOperation invocation)
    {
        static ITypeSymbol? getSymbolFromOperation(IOperation? operand)
        {
            return operand switch
            {
                IFieldReferenceOperation field => field.Type,
                IPropertyReferenceOperation prop => prop.Type,
                IMethodReferenceOperation method => method.Type,
                ILocalReferenceOperation local => local.Local.Type,
                IParameterReferenceOperation param => param.Parameter.Type,
                IObjectCreationOperation objectCreation => objectCreation.Type,
                _ => default
            };
        }

        var instance = getSymbolFromOperation(invocation.Instance);
        if (instance != default)
        {
            return instance;
        }

        // Otherwise, it's a static call
        if (invocation.Arguments is { Length: > 0 })
        {
            var firstArgument = invocation.Arguments[0].Value;

            // Regular static method
            if (firstArgument is IConversionOperation { Operand: IMemberReferenceOperation or ILocalReferenceOperation or IParameterReferenceOperation or IObjectCreationOperation } conv)
            {
                return getSymbolFromOperation(conv.Operand);
            }

            // Object creation
            if (firstArgument is IInvocationOperation inv)
            {
                return GetTypeOfInstanceInInvocation(inv);
            }
        }

        return default;
    }

    public static (IMethodSymbol? SurroundingMethodSymbol, IOperation SurroundingMethodOperation) GetSurroundingMethodContext(this IOperation operation)
    {
        var surroundingMethodOperation = operation.Ancestors().FirstOrDefault(a => a is ILocalFunctionOperation or IMethodBodyBaseOperation or IAnonymousFunctionOperation);
        return surroundingMethodOperation switch
        {
            ILocalFunctionOperation localFunction => (localFunction.Symbol, localFunction),
            IMethodBodyBaseOperation methodBody => (methodBody.SemanticModel?.GetDeclaredSymbol(methodBody.Syntax) as IMethodSymbol, methodBody),
            IAnonymousFunctionOperation anonFunction => (anonFunction.Symbol, anonFunction),
            _ => default,
        };
    }

    public static bool IsInsideLockStatement(this IOperation operation) => operation.Ancestors().Any(a => a is ILockOperation);
}