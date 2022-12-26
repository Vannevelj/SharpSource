using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpSource.Utilities;

public static class Extensions
{
    private static readonly Dictionary<string, string> AliasMapping = new()
    {
        { nameof(Int16), "short" },
        { nameof(Int32), "int" },
        { nameof(Int64), "long" },
        { nameof(UInt16), "ushort" },
        { nameof(UInt32), "uint" },
        { nameof(UInt64), "ulong" },
        { nameof(Object), "object" },
        { nameof(Byte), "byte" },
        { nameof(SByte), "sbyte" },
        { nameof(Char), "char" },
        { nameof(Boolean), "bool" },
        { nameof(Single), "float" },
        { nameof(Double), "double" },
        { nameof(Decimal), "decimal" },
        { nameof(String), "string" }
    };

    public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol typeSymbol, Type interfaceType)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        if (typeSymbol.BaseType?.MetadataName == interfaceType.Name)
        {
            return true;
        }

        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            if (@interface.MetadataName == interfaceType.Name)
            {
                return true;
            }
        }

        return false;
    }

    public static bool ImplementsInterface(this INamedTypeSymbol typeSymbol, Type interfaceType)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        foreach (var @interface in typeSymbol.AllInterfaces)
        {
            if (@interface.MetadataName == interfaceType.Name)
            {
                return true;
            }
        }

        return false;
    }

    public static bool InheritsFrom(this ISymbol typeSymbol, Type type)
    {
        if (typeSymbol == null || type == null)
        {
            return false;
        }

        var baseType = typeSymbol;
        while (baseType != null && baseType.MetadataName != typeof(object).Name &&
               baseType.MetadataName != typeof(ValueType).Name)
        {
            if (baseType.MetadataName == type.Name)
            {
                return true;
            }
            baseType = ( (ITypeSymbol)baseType ).BaseType;
        }

        return false;
    }

    [SuppressMessage("Correctness", "SS018:Add cases for missing enum member.", Justification = "Too many SyntaxKind enum members")]
    public static bool IsCommentTrivia(this SyntaxTrivia trivia)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
            case SyntaxKind.DocumentationCommentExteriorTrivia:
            case SyntaxKind.SingleLineDocumentationCommentTrivia:
            case SyntaxKind.MultiLineDocumentationCommentTrivia:
            case SyntaxKind.EndOfDocumentationCommentToken:
            case SyntaxKind.XmlComment:
            case SyntaxKind.XmlCommentEndToken:
            case SyntaxKind.XmlCommentStartToken:
                return true;
            default:
                return false;
        }
    }

    public static bool IsWhitespaceTrivia(this SyntaxTrivia trivia) => trivia.Kind() switch
    {
        SyntaxKind.WhitespaceTrivia or SyntaxKind.EndOfLineTrivia => true,
        _ => false,
    };

    public static string ToAlias(this string type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (AliasMapping.TryGetValue(type, out var foundValue))
        {
            return foundValue;
        }

        throw new ArgumentException("Could not find the type specified", nameof(type));
    }

    public static bool HasAlias(this string type, out string alias)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return AliasMapping.TryGetValue(type, out alias);
    }

    /// <summary>
    ///     Determines whether or not the specified <see cref="IMethodSymbol" /> is the symbol of an asynchronous method. This
    ///     can be a method declared as async (e.g. returning <see cref="Task" /> or <see cref="Task{TResult}" />), or a method
    ///     with an async implementation (using the <code>async</code> keyword).
    /// </summary>
    public static bool IsAsync(this IMethodSymbol methodSymbol) => methodSymbol.IsAsync ||
               methodSymbol.ReturnType.MetadataName == typeof(Task).Name ||
               methodSymbol.ReturnType.MetadataName == typeof(Task<>).Name;

    public static bool IsDefinedInAncestor(this IMethodSymbol methodSymbol)
    {
        var containingType = methodSymbol.ContainingType;
        if (containingType == null)
        {
            return false;
        }

        var interfaces = containingType.AllInterfaces;
        foreach (var @interface in interfaces)
        {
            var interfaceMethods = @interface.GetMembers().Select(containingType.FindImplementationForInterfaceMember);

            foreach (var method in interfaceMethods)
            {
                if (method != null && method.Equals(methodSymbol, SymbolEqualityComparer.Default))
                {
                    return true;
                }
            }
        }

        var baseType = containingType.BaseType;
        while (baseType != null)
        {
            var baseMethods = baseType.GetMembers().OfType<IMethodSymbol>();

            foreach (var method in baseMethods)
            {
                if (method.Equals(methodSymbol.OverriddenMethod, SymbolEqualityComparer.Default))
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

    public static ITypeSymbol? GetConcreteTypeOfInvocation(this SyntaxNode invocation, SemanticModel semanticModel)
    {
        ExpressionSyntax? invokedExpression = invocation switch
        {
            ConditionalAccessExpressionSyntax conditionalAccessExpression => conditionalAccessExpression.WhenNotNull,
            PostfixUnaryExpressionSyntax postfixUnaryExpression when postfixUnaryExpression.IsKind(SyntaxKind.SuppressNullableWarningExpression) => postfixUnaryExpression.Operand,
            InvocationExpressionSyntax invocationExpression => invocationExpression.Expression,
            _ => default
        };

        if (invokedExpression is MemberAccessExpressionSyntax memberAccessExpression) 
        {
            var invokedType = semanticModel.GetTypeInfo(memberAccessExpression.Expression);
            return invokedType.Type;
        }

        return default;
    }

    public static bool IsNameofInvocation(this InvocationExpressionSyntax invocation)
    {
        if (invocation == null)
        {
            throw new ArgumentNullException(nameof(invocation));
        }

        var identifier = invocation.Expression.DescendantNodesAndSelf()
                                   .OfType<IdentifierNameSyntax>()
                                   .FirstOrDefault();

        return identifier != null && identifier.Identifier.ValueText == "nameof";
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

    public static T FirstOfKind<T>(this IEnumerable<SyntaxNode> enumerable, SyntaxKind kind) where T : SyntaxNode => enumerable.OfType<T>(kind).FirstOrDefault();

    public static bool ContainsAny(this SyntaxTokenList list, params SyntaxKind[] kinds)
    {
        foreach (var item in list)
        {
            foreach (var kind in kinds)
            {
                if (item.IsKind(kind))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool Contains(this SyntaxTokenList list, SyntaxKind kind)
    {
        foreach (var item in list)
        {
            if (item.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    public static bool Contains(this IEnumerable<SyntaxKind> list, SyntaxKind kind)
    {
        foreach (var syntaxKind in list)
        {
            if (syntaxKind == kind)
            {
                return true;
            }
        }

        return false;
    }

    public static bool Any(this IEnumerable<SyntaxNode> list, SyntaxKind kind)
    {
        foreach (var node in list)
        {
            if (node.IsKind(kind))
            {
                return true;
            }
        }

        return false;
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

    public static CompilationUnitSyntax AddUsingStatementIfMissing(this CompilationUnitSyntax compilation, string import)
    {
        if (!compilation.Usings.Any(x => x.Name.GetText().ToString() == import))
        {
            var parts = import.Split('.').Select(x => SyntaxFactory.IdentifierName(x)).ToList();
            if (parts.Count == 1)
            {
                return compilation.AddUsings(SyntaxFactory.UsingDirective(parts[0]));
            }

            var counter = 0;
            NameSyntax currentName = parts[0];
            while (counter < parts.Count - 1)
            {
                currentName = SyntaxFactory.QualifiedName(currentName, parts[counter + 1]);
                counter++;
            }

            return compilation.AddUsings(SyntaxFactory.UsingDirective(currentName));
        }

        return compilation;
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

    public static IEnumerable<INamedTypeSymbol> GetAttributesOfType(this IEnumerable<INamedTypeSymbol?> attributes, Type type) =>
        attributes.Where(a => a?.MetadataName == type.Name && a.IsDefinedInSystemAssembly())!;

    public static IMethodSymbol GetBaseDefinition(this IMethodSymbol method)
    {
        if (!method.IsOverride)
        {
            return method;
        }

        while (method.IsOverride && method.OverriddenMethod != default)
        {
            method = method.OverriddenMethod;
        }

        return method;
    }

    public static ISymbol? GetCreatedType(this BaseObjectCreationExpressionSyntax expression, SemanticModel semanticModel) =>
        expression switch
        {
            ObjectCreationExpressionSyntax objectCreation => semanticModel.GetSymbolInfo(objectCreation.Type).Symbol,
            ImplicitObjectCreationExpressionSyntax implicitObjectCreation => semanticModel.GetSymbolInfo(implicitObjectCreation).Symbol?.ContainingSymbol,
            _ => default
        };

    /// <summary>
    /// Removes the first instance of a particular method call from a chain of invocations
    /// </summary>
    /// <param name="unwrapSuppress">Turns <c>value!.ToString()</c> into <c>value</c> if set to <c>true</c>. Otherwise <c>value!</c></param>
    public static SyntaxNode RemoveInvocation(this SyntaxNode invocationOrConditionalAccess, Type type, string method, SemanticModel semanticModel, bool unwrapSuppress = false)
    {
        ExpressionSyntax updateName(ExpressionSyntax subExpression, ExpressionSyntax nextInvocation)
        {
            var nextInvocationName = nextInvocation switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
                _ => throw new ArgumentException()
            };


            ExpressionSyntax newExpression = subExpression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(nextInvocationName),
                MemberBindingExpressionSyntax memberBinding => memberBinding.WithName(nextInvocationName),
                _ => throw new ArgumentException()
            };
            return newExpression;
        }

        // s1?.ToLower()
        if (invocationOrConditionalAccess is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            var fullInvocation = conditionalAccess.WhenNotNull.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            var firstInvocation = fullInvocation?.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (firstInvocation == default || fullInvocation == default)
            {
                return conditionalAccess.Expression;
            }

            var subExpression = firstInvocation.Expression;
            var nextInvocation = fullInvocation.Expression;
            return conditionalAccess.WithWhenNotNull(fullInvocation.WithExpression(updateName(subExpression, nextInvocation)));
        }

        foreach (InvocationExpressionSyntax nestedInvocation in invocationOrConditionalAccess.DescendantNodesAndSelfOfType(SyntaxKind.InvocationExpression))
        {
            if (!nestedInvocation.IsAnInvocationOf(type, method, semanticModel))
            {
                continue;
            }

            var newExpression = nestedInvocation.Expression switch
            {
                // s1!.ToLower()
                MemberAccessExpressionSyntax memberAccessSuppressing when
                    memberAccessSuppressing.Expression is PostfixUnaryExpressionSyntax postfixUnary &&
                    postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression) => unwrapSuppress ? postfixUnary.Operand : postfixUnary,

                // s1.ToLower()
                MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,

                _ => nestedInvocation.Expression
            };

            if (newExpression == default)
            {
                continue;
            }

            var surroundingInvocation = nestedInvocation.FirstAncestorOrSelfUntil<InvocationExpressionSyntax>(node => node == invocationOrConditionalAccess);
            if (surroundingInvocation == default || invocationOrConditionalAccess == nestedInvocation)
            {
                return newExpression;
            }

            var newNode = invocationOrConditionalAccess.ReplaceNode(nestedInvocation, newExpression);
            if (newNode != default)
            {
                return newNode;
            }

            return newExpression;
        }

        return invocationOrConditionalAccess;
    }

    public static IEnumerable<SyntaxNode> DescendantNodesAndSelfOfType(this SyntaxNode node, params SyntaxKind[] kinds)
    {
        foreach (var descendant in node.DescendantNodesAndSelf())
        {
            if (descendant.IsAnyKind(kinds))
            {
                yield return descendant;
            }
        }
    }

    public static TNode? FirstAncestorOrSelfUntil<TNode>(this SyntaxNode node, Func<SyntaxNode, bool> predicate) where TNode : SyntaxNode
    {
        var parent = node;
        while (parent != default)
        {
            if (parent is TNode)
            {
                return (TNode?)parent;
            }

            if (predicate(node))
            {
                return default;
            }

            parent = parent.Parent;
        }

        return default;
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

    public static IEnumerable<SyntaxNode> GetStatements(this BaseMethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Body != default)
        {
            foreach (var statement in methodDeclaration.Body.Statements)
            {
                yield return statement;
            }
        }

        if (methodDeclaration.ExpressionBody != default)
        {
            yield return methodDeclaration.ExpressionBody.Expression;
        }
    }

    public static (string? Name, bool? IsNullable) GetCancellationTokenFromParameters(this IMethodSymbol? method)
    {
        if (method == default)
        {
            return default;
        }

        foreach (var parameter in method.Parameters)
        {
            if (parameter.Type is INamedTypeSymbol { Name: "Nullable", Arity: 1 } ctoken && ctoken.TypeArguments.Single().Name == "CancellationToken")
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

    public static bool PassesThroughCancellationToken(this InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            var argumentType = semanticModel.GetTypeInfo(argument.Expression).Type;

            if (argumentType is INamedTypeSymbol { Name: "Nullable", Arity: 1 } ctoken && ctoken.TypeArguments.Single().Name == "CancellationToken")
            {
                return true;
            }

            if (argumentType is INamedTypeSymbol { Name: "CancellationToken" })
            {
                return true;
            }
        }

        return false;
    }
}