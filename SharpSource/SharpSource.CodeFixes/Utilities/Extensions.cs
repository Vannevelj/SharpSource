using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpSource.Utilities;

public static class Extensions
{
    public static T FirstOfKind<T>(this IEnumerable<SyntaxNode> enumerable, SyntaxKind kind) where T : SyntaxNode => enumerable.OfType<T>(kind).FirstOrDefault();

    /// <summary>
    /// Removes the first instance of a particular method call from a chain of invocations
    /// </summary>
    /// <param name="unwrapSuppress">Turns <c>value!.ToString()</c> into <c>value</c> if set to <c>true</c>. Otherwise <c>value!</c></param>
    public static SyntaxNode RemoveInvocation(this SyntaxNode invocationOrConditionalAccess, Type type, string method, SemanticModel semanticModel, bool unwrapSuppress = false)
    {
        // Handle conditional access chains like s1?.Trim()?.ToLower() or s1?.ToLower()
        if (invocationOrConditionalAccess is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            // Find the innermost invocation that matches our target method
            var targetInvocation = conditionalAccess.DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .Where(x => x.ArgumentList.Arguments.Count == 0)
                .FirstOrDefault(x => x.IsAnInvocationOf(type, method, semanticModel));

            if (targetInvocation == default)
            {
                return conditionalAccess.Expression;
            }

            // Find the ConditionalAccessExpressionSyntax that directly contains this invocation
            var containingConditional = targetInvocation.Ancestors()
                .OfType<ConditionalAccessExpressionSyntax>()
                .FirstOrDefault();

            if (containingConditional == default)
            {
                return conditionalAccess.Expression;
            }

            // If the target invocation is directly in WhenNotNull (e.g., s1?.ToLower())
            // we need to check if there's a chain before it
            var whenNotNull = containingConditional.WhenNotNull;

            // Check if the invocation is at the end of a member binding chain
            // e.g., s1?.Trim()?.ToLower() where ToLower() is at the end
            if (whenNotNull is InvocationExpressionSyntax invExpr &&
                invExpr == targetInvocation &&
                invExpr.Expression is MemberBindingExpressionSyntax)
            {
                // The entire WhenNotNull is just the target invocation with member binding
                // So we remove this conditional access level entirely
                if (containingConditional == conditionalAccess)
                {
                    return conditionalAccess.Expression;
                }

                // Find what remains after removing this conditional access
                var newRoot = conditionalAccess.ReplaceNode(containingConditional, containingConditional.Expression);
                return newRoot;
            }

            // For chains like s1?.Trim().ToLower() where it's member access (not binding)
            // Or s1?.Trim()?.ToLower() where we found the inner one
            var newExpression = targetInvocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Expression,
                MemberBindingExpressionSyntax => null, // Handled above
                _ => null
            };

            if (newExpression != null)
            {
                var newNode = conditionalAccess.ReplaceNode(targetInvocation, newExpression);
                return newNode;
            }

            return conditionalAccess.Expression;
        }

        foreach (var nestedInvocation in invocationOrConditionalAccess.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.ArgumentList.Arguments.Count == 0))
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

    public static async ValueTask<SyntaxNode> GetRequiredSyntaxRootAsync(this Document document, CancellationToken cancellationToken)
    {
        if (document.TryGetSyntaxRoot(out var root))
        {
            return root;
        }

        root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        return root ?? throw new InvalidOperationException($"Unable to find a syntax root for document {document.Name}");
    }

    public static SyntaxNode? GetOuterParentOfType(this SyntaxNode node, params SyntaxKind[] types)
    {
        var currentNode = node.Parent;
        while (currentNode?.Parent?.IsAnyKind(types) == true)
        {
            currentNode = currentNode.Parent;
        }
        return currentNode;
    }
}