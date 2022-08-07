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

        public static DiagnosticDescriptor Rule => new(DiagnosticId.AsyncOverloadsAvailable, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                switch (invocation.Expression)
                {
                    case MemberAccessExpressionSyntax memberAccess:
                        CheckIfOverloadAvailable(memberAccess.Name, context);
                        break;
                    case IdentifierNameSyntax identifierName:
                        CheckIfOverloadAvailable(identifierName, context);
                        break;
                    case GenericNameSyntax genericName:
                        CheckIfOverloadAvailable(genericName, context);
                        break;
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
                if (overload.Parameters.Length != invokedMethod.Parameters.Length && overload.Parameters.Any())
                {
                    // We allow overloads to differ by providing a cancellationtoken
                    var lastParameter = overload.Parameters.Last();
                    hasSameParameters =
                        overload.Parameters.Length - 1 == invokedMethod.Parameters.Length &&
                        lastParameter.Type is INamedTypeSymbol { Name: "Nullable", Arity: 1 } ctoken &&
                        ctoken.TypeArguments.Single().Name == "CancellationToken";
                }

                if (invokedMethod.Parameters.Length <= overload.Parameters.Length)
                {
                    for (var i = 0; i < invokedMethod.Parameters.Length; i++)
                    {
                        if (!invokedMethod.Parameters[i].Type.Equals(overload.Parameters[i].Type, SymbolEqualityComparer.Default))
                        {
                            hasSameParameters = false;
                            break;
                        }
                    }
                }

                if (hasSameParameters)
                {
                    var isVoidOverload = returnType.SpecialType == SpecialType.System_Void && overload.ReturnType.IsNonGenericTaskType();
                    var isGenericOverload =
                        returnType.SpecialType != SpecialType.System_Void &&
                        overload.ReturnType.IsGenericTaskType(out var wrappedType) &&
                        ( wrappedType.Equals(returnType, SymbolEqualityComparer.Default) || wrappedType.TypeKind == TypeKind.TypeParameter );

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