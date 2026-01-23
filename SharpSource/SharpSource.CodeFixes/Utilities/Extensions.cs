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

            // Check if the target invocation uses member binding (e.g., ?.ToArray())
            if (targetInvocation.Expression is MemberBindingExpressionSyntax)
            {
                // Find the ConditionalAccessExpressionSyntax that directly contains this invocation as its WhenNotNull
                var containingConditional = targetInvocation.Ancestors()
                    .OfType<ConditionalAccessExpressionSyntax>()
                    .FirstOrDefault(ca => ca.WhenNotNull == targetInvocation);

                if (containingConditional != null)
                {
                    // The target is directly in WhenNotNull - remove this conditional access level
                    // e.g., for s1?.Trim()?.ToLower(), if ToLower is the target and it's the WhenNotNull of
                    // the inner conditional, replace that inner conditional with just its Expression
                    if (containingConditional == conditionalAccess)
                    {
                        // The target is the WhenNotNull of the outermost conditional access
                        return conditionalAccess.Expression;
                    }
                    else
                    {
                        // The target is in a nested conditional access - replace the nested conditional with its Expression
                        var newNode = conditionalAccess.ReplaceNode(containingConditional, containingConditional.Expression);
                        return newNode;
                    }
                }

                // If there's a chain after the target (e.g., values?.ToArray().ToList())
                // we need to convert the member binding to member access on the expression
                var parent = targetInvocation.Parent;
                if (parent is MemberAccessExpressionSyntax memberAccess)
                {
                    // The target invocation is being accessed by something after it
                    // e.g., in .ToArray().ToList(), ToArray() is accessed by .ToList
                    // Replace ToArray() invocation with a member binding expression
                    var newMemberBinding = SyntaxFactory.MemberBindingExpression(memberAccess.Name);
                    var newWhenNotNull = conditionalAccess.WhenNotNull.ReplaceNode(memberAccess, newMemberBinding);
                    return conditionalAccess.WithWhenNotNull(newWhenNotNull);
                }

                return conditionalAccess.Expression;
            }

            // For chains like s1?.Trim().ToLower() where ToLower uses member access (not binding)
            if (targetInvocation.Expression is MemberAccessExpressionSyntax targetMemberAccess)
            {
                var newNode = conditionalAccess.ReplaceNode(targetInvocation, targetMemberAccess.Expression);
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