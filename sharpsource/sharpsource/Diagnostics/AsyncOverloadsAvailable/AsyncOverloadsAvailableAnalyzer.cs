using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.CorrectTPLMethodsInAsyncContext
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncOverloadsAvailableAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = Resources.AsyncOverloadsAvailableMessage;
        private static readonly string Title = Resources.AsyncOverloadsAvailableTitle;

        public static DiagnosticDescriptor Rule => new DiagnosticDescriptor(DiagnosticId.AsyncOverloadsAvailable, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) => context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    CheckIfOverloadAvailable(memberAccess.Name, context);
                }
                else if (invocation.Expression is IdentifierNameSyntax identifierName)
                {
                    CheckIfOverloadAvailable(identifierName, context);
                }
            }
        }

        private void CheckIfOverloadAvailable(SimpleNameSyntax invokedFunction, SyntaxNodeAnalysisContext context)
        {
            var invokedSymbol = context.SemanticModel.GetSymbolInfo(invokedFunction).Symbol;
            if (invokedSymbol == null)
            {
                return;
            }

            var invokedMethodName = invokedSymbol.Name;
            var invokedTypeName = invokedSymbol.ContainingType?.Name;

            var methodsInInvokedType = invokedSymbol.ContainingType.GetMembers().OfType<IMethodSymbol>();
            var relevantOverloads = methodsInInvokedType.Where(x => x.Name == $"{invokedMethodName}Async");

            if (!( invokedSymbol is IMethodSymbol invokedMethod ))
            {
                return;
            }

            var returnType = invokedMethod.ReturnType;

            foreach (var overload in relevantOverloads)
            {
                var hasSameParameters = true;
                if (overload.Parameters.Length != invokedMethod.Parameters.Length)
                {
                    // We allow overloads to differ by providing a cancellationtoken
                    var lastParameter = overload.Parameters.Last();
                    hasSameParameters =
                        overload.Parameters.Length - 1 == invokedMethod.Parameters.Length &&
                        lastParameter.Type is INamedTypeSymbol { Name: "Nullable", Arity: 1 } ctoken &&
                        ctoken.TypeArguments.Single().Name == "CancellationToken";
                }

                for (var i = 0; i < invokedMethod.Parameters.Length; i++)
                {
                    if (!invokedMethod.Parameters[i].Type.Equals(overload.Parameters[i].Type))
                    {
                        hasSameParameters = false;
                        break;
                    }
                }

                if (hasSameParameters)
                {
                    var isVoidOverload = returnType.SpecialType == SpecialType.System_Void && overload.ReturnType.IsNonGenericTaskType();
                    var isGenericOverload = returnType.SpecialType != SpecialType.System_Void && overload.ReturnType.IsGenericTaskType(out var wrappedType) && wrappedType.Equals(returnType);

                    if (isVoidOverload || isGenericOverload)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, invokedFunction.GetLocation(), $"{invokedTypeName}.{invokedMethodName}"));
                        return;
                    }
                }
            }
        }
    }
}
