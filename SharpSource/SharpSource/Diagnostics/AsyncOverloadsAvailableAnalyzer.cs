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
                CheckIfOverloadAvailable(memberAccess.Name, context, surroundingDeclaration);
                break;
            case IdentifierNameSyntax identifierName:
                CheckIfOverloadAvailable(identifierName, context, surroundingDeclaration);
                break;
            case GenericNameSyntax genericName:
                CheckIfOverloadAvailable(genericName, context, surroundingDeclaration);
                break;
            default:
                break;
        }
    }

    private void CheckIfOverloadAvailable(SimpleNameSyntax invokedFunction, SyntaxNodeAnalysisContext context, SyntaxNode surroundingDeclaration)
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
                properties.Add("currentContextHasOptionalCancellationToken", IsOptionalCancellationToken(surroundingMethodDeclaration?.Parameters.LastOrDefault()?.Type) ? "true" : "false");
                properties.Add("currentContextHasCancellationToken", MethodHasCancellationTokenAsLastParameter(surroundingMethodDeclaration) ? "true" : "false");
                properties.Add("currentInvocationHasCancellationToken", MethodHasCancellationTokenAsLastParameter(invokedMethod) ? "true" : "false");
                properties.Add("newInvocationAcceptsCancellationToken", MethodHasCancellationTokenAsLastParameter(overload) ? "true" : "false");
                context.ReportDiagnostic(Diagnostic.Create(Rule, invokedFunction.GetLocation(), properties.ToImmutable(), $"{invokedTypeName}.{invokedMethodName}"));
            }
        }
    }

    private static bool IsOptionalCancellationToken(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "Nullable", Arity: 1 } ctoken &&
        ctoken.TypeArguments.Single().Name == "CancellationToken";

    private static bool IsRequiredCancellationToken(ITypeSymbol? type) =>
        type is INamedTypeSymbol { Name: "CancellationToken" };

    private static bool HasOneAdditionalOptionalCancellationTokenParameter(IMethodSymbol invokedMethod, IMethodSymbol overload) =>
        invokedMethod.Parameters.Length == overload.Parameters.Length - 1 &&
        IsOptionalCancellationToken(overload.Parameters.Last().Type);

    private static bool HasOneAdditionaRequiredCancellationTokenParameter(IMethodSymbol invokedMethod, IMethodSymbol overload) =>
        invokedMethod.Parameters.Length == overload.Parameters.Length - 1 &&
        IsRequiredCancellationToken(overload.Parameters.Last().Type); 

    private static bool MethodHasCancellationTokenAsLastParameter(IMethodSymbol? invokedMethod) =>
        invokedMethod is { Parameters.Length: >= 1} &&
        (   IsOptionalCancellationToken(invokedMethod.Parameters.Last().Type) ||
            IsRequiredCancellationToken(invokedMethod.Parameters.Last().Type));

    private static bool IsIdenticalOverload(IMethodSymbol invokedMethod, IMethodSymbol overload, IMethodSymbol? surroundingMethodDeclaration)
    {
        var hasExactSameNumberOfParameters = invokedMethod.Parameters.Length == overload.Parameters.Length;
        var hasOneAdditionalCancellationTokenParameter = HasOneAdditionalOptionalCancellationTokenParameter(invokedMethod, overload);
        var hasACancellationTokenToPassThrough = HasOneAdditionaRequiredCancellationTokenParameter(invokedMethod, overload) && MethodHasCancellationTokenAsLastParameter(surroundingMethodDeclaration);

        // We allow overloads to differ by providing a cancellationtoken
        var isParameterCountOkay = hasExactSameNumberOfParameters || hasOneAdditionalCancellationTokenParameter || hasACancellationTokenToPassThrough;
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