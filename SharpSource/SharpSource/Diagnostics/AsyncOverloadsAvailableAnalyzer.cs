using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncOverloadsAvailableAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.AsyncOverloadsAvailable,
        "An async overload is available",
        "Async overload available for {0}",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS033-AsyncOverloadsAvailable.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var surroundingDeclaration = invocation.FirstAncestorOrSelfOfType(SyntaxKind.MethodDeclaration, SyntaxKind.GlobalStatement, SyntaxKind.SimpleLambdaExpression);

        var isInCorrectContext = surroundingDeclaration switch
        {
            MethodDeclarationSyntax method => method.Modifiers.Any(SyntaxKind.AsyncKeyword),
            GlobalStatementSyntax => true,
            SimpleLambdaExpressionSyntax lambda => lambda.Modifiers.Any(SyntaxKind.AsyncKeyword),
            _ => false
        };

        if (!isInCorrectContext || surroundingDeclaration == default)
        {
            return;
        }

        switch (invocation.Expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                CheckIfOverloadAvailable(memberAccess.Name, context, surroundingDeclaration, invocation);
                break;
            case IdentifierNameSyntax identifierName:
                CheckIfOverloadAvailable(identifierName, context, surroundingDeclaration, invocation);
                break;
            case GenericNameSyntax genericName:
                CheckIfOverloadAvailable(genericName, context, surroundingDeclaration, invocation);
                break;
            default:
                break;
        }
    }

    private static void CheckIfOverloadAvailable(SimpleNameSyntax invokedFunction, SyntaxNodeAnalysisContext context, SyntaxNode surroundingDeclaration, InvocationExpressionSyntax invocation)
    {
        var invokedSymbol = context.SemanticModel.GetSymbolInfo(invokedFunction).Symbol;
        if (invokedSymbol?.ContainingType == default)
        {
            return;
        }

        var invokedMethodName = invokedSymbol.Name;
        var invokedTypeName = invokedSymbol.ContainingType.Name;

        var methodsInInvokedType = invokedSymbol.ContainingType.GetMembers().OfType<IMethodSymbol>();
        var relevantOverloads = methodsInInvokedType.Where(x => x.Name == $"{invokedMethodName}Async");

        if (invokedSymbol is not IMethodSymbol invokedMethod)
        {
            return;
        }

        var surroundingMethodDeclaration = context.SemanticModel.GetDeclaredSymbol(surroundingDeclaration) as IMethodSymbol;
        foreach (var overload in relevantOverloads)
        {
            if (IsIdenticalOverload(invokedMethod, overload, surroundingMethodDeclaration))
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                var (cancellationTokenName, cancellationTokenIsNullable) = surroundingMethodDeclaration.GetCancellationTokenFromParameters();

                var currentInvocationPassesCancellationToken = invocation.PassesThroughCancellationToken(context.SemanticModel);
                var newInvocationAcceptsCancellationToken = overload.GetCancellationTokenFromParameters() != default;

                properties.Add("cancellationTokenName", cancellationTokenName);
                properties.Add("cancellationTokenIsOptional", cancellationTokenIsNullable == true ? "true" : "false");

                var shouldAddCancellationToken = cancellationTokenName != default && !currentInvocationPassesCancellationToken && newInvocationAcceptsCancellationToken;
                properties.Add("shouldAddCancellationToken", shouldAddCancellationToken ? "true" : "false");
                context.ReportDiagnostic(Diagnostic.Create(Rule, invokedFunction.GetLocation(), properties.ToImmutable(), $"{invokedTypeName}.{invokedMethodName}"));
            }
        }
    }

    private static bool IsIdenticalOverload(IMethodSymbol invokedMethod, IMethodSymbol overload, IMethodSymbol? surroundingMethodDeclaration)
    {
        /**
         * Three variables in play:
         *  - The current context, i.e. the method surrounding our call
         *  - The currently invoked function, i.e. Get()
         *  - The potential overload, i.e. GetAsync()
         * 
         * If the current context doesn't provide a cancellationtoken, the overload must not require it either (no parameter or optional ctoken)
         * If the current context does provide a cancellationtoken and the overload accepts one (optional or required), we pass it through
         * If the current context does provide a cancellationtoken but the overload doesn't accept one, we don't pass it through
         * If the current context does provide a cancellationtoken and the current invocation uses it but the overload doesn't accept it, we need to remove it         * 
         **/

        var hasExactSameNumberOfParameters = invokedMethod.Parameters.Length == overload.Parameters.Length;
        var hasOneAdditionalParameter = invokedMethod.Parameters.Length == overload.Parameters.Length - 1;
        var hasOneAdditionalOptionalCancellationTokenParameter = hasOneAdditionalParameter && overload.GetCancellationTokenFromParameters().IsNullable == true;
        var hasOneAdditionalRequiredCancellationTokenParameter = hasOneAdditionalParameter && overload.GetCancellationTokenFromParameters().IsNullable == false && surroundingMethodDeclaration.GetCancellationTokenFromParameters() != default;

        // We allow overloads to differ by providing a cancellationtoken
        var isParameterCountOkay = hasExactSameNumberOfParameters || hasOneAdditionalOptionalCancellationTokenParameter || hasOneAdditionalRequiredCancellationTokenParameter;
        if (!isParameterCountOkay)
        {
            return false;
        }

        for (var i = 0; i < invokedMethod.Parameters.Length; i++)
        {
            if (!invokedMethod.Parameters[i].Type.Equals(overload.Parameters[i].Type, SymbolEqualityComparer.IncludeNullability))
            {
                return false;
            }
        }

        var returnType = invokedMethod.ReturnType;
        var isVoidOverload = returnType.SpecialType == SpecialType.System_Void && overload.ReturnType.IsNonGenericTaskType();
        var isGenericOverload =
            returnType.SpecialType != SpecialType.System_Void &&
            overload.ReturnType.IsGenericTaskType(out var wrappedType) &&
            wrappedType != default &&
            ( wrappedType.Equals(returnType, SymbolEqualityComparer.Default) || wrappedType.TypeKind == TypeKind.TypeParameter );
        var isSurroundingMethod = overload.Equals(surroundingMethodDeclaration, SymbolEqualityComparer.Default);

        return ( isVoidOverload || isGenericOverload ) && !isSurroundingMethod;
    }
}