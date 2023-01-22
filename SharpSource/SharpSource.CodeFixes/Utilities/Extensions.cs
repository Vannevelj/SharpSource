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
        static ExpressionSyntax updateName(ExpressionSyntax subExpression, ExpressionSyntax nextInvocation)
        {
            var nextInvocationName = nextInvocation switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
                _ => throw new ArgumentException("Invalid invocation expression")
            };


            ExpressionSyntax newExpression = subExpression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.WithName(nextInvocationName),
                MemberBindingExpressionSyntax memberBinding => memberBinding.WithName(nextInvocationName),
                _ => throw new ArgumentException("Invalid invocation expression")
            };
            return newExpression;
        }

        // s1?.ToLower()
        if (invocationOrConditionalAccess is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            var fullInvocation = conditionalAccess.WhenNotNull.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.ArgumentList.Arguments.Count == 0).FirstOrDefault();
            var firstInvocation = fullInvocation?.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (firstInvocation == default || fullInvocation == default)
            {
                return conditionalAccess.Expression;
            }

            var subExpression = firstInvocation.Expression;
            var nextInvocation = fullInvocation.Expression;
            return conditionalAccess.WithWhenNotNull(fullInvocation.WithExpression(updateName(subExpression, nextInvocation)));
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