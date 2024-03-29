using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var cancellationTokenSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
            compilationContext.RegisterOperationAction((context) => Analyze(context, cancellationTokenSymbol), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol? cancellationTokenSymbol)
    {
        var (surroundingMethod, surroundMethodOperation) = context.Operation.GetSurroundingMethodContext();
        if (surroundingMethod is null)
        {
            return;
        }

        // If the surrounding method is a global statement it is considered as `IsAsync: false` even though async calls work
        if (surroundingMethod is { IsAsync: false, Name: not WellKnownMemberNames.TopLevelStatementsEntryPointMethodName })
        {
            return;
        }

        var invocation = (IInvocationOperation)context.Operation;
        if (invocation.IsInsideLockStatement())
        {
            return;
        }

        var invokedMethodName = invocation.TargetMethod.Name;
        var invokedTypeName = invocation.TargetMethod.ContainingType.Name;

        var relevantOverloads = invocation.TargetMethod.ContainingType.GetMembers($"{invokedMethodName}Async").OfType<IMethodSymbol>();

        var (cancellationTokenName, cancellationTokenIsNullable) = surroundingMethod.GetCancellationTokenFromParameters();
        if (cancellationTokenName is null && surroundMethodOperation is ILocalFunctionOperation { Symbol.IsStatic: false })
        {
            // if it is a non-static local function, we can pass through the cancellation token that's in the surrounding method
            var (outerMethodSymbol, _) = surroundMethodOperation.GetSurroundingMethodContext();
            (cancellationTokenName, cancellationTokenIsNullable) = outerMethodSymbol.GetCancellationTokenFromParameters();
        }

        foreach (var overload in relevantOverloads)
        {
            if (IsIdenticalOverload(invocation.TargetMethod, overload, surroundingMethod, hasCancellationTokenInContext: cancellationTokenName != default))
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();

                var currentInvocationPassesCancellationToken = invocation.PassesThroughCancellationToken(cancellationTokenSymbol);
                var newInvocationAcceptsCancellationToken = overload.GetCancellationTokenFromParameters() != default;

                properties.Add("cancellationTokenName", cancellationTokenName);
                properties.Add("cancellationTokenIsOptional", cancellationTokenIsNullable == true ? "true" : "false");

                var shouldAddCancellationToken = cancellationTokenName != default &&
                                                 !currentInvocationPassesCancellationToken &&
                                                 newInvocationAcceptsCancellationToken;
                properties.Add("shouldAddCancellationToken", shouldAddCancellationToken ? "true" : "false");
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), $"{invokedTypeName}.{invokedMethodName}"));
                return;
            }
        }
    }

    private static bool IsIdenticalOverload(IMethodSymbol invokedMethod, IMethodSymbol overload, IMethodSymbol surroundingMethodDeclaration, bool hasCancellationTokenInContext)
    {
        /**
         * Three variables in play:
         *  - The current context, i.e. the method surrounding our call
         *  - The currently invoked function, i.e. Get()
         *  - The potential overload, i.e. GetAsync()
         * 
         * If the current context doesn't provide a cancellationToken, the overload must not require it either (no parameter or optional ctoken)
         * If the current context does provide a cancellationToken and the overload accepts one (optional or required), we pass it through
         * If the current context does provide a cancellationToken but the overload doesn't accept one, we don't pass it through
         * If the current context does provide a cancellationToken and the current invocation uses it but the overload doesn't accept it, we need to remove it
         **/

        var hasExactSameNumberOfParameters = invokedMethod.Parameters.Length == overload.Parameters.Length;
        var hasOneAdditionalParameter = invokedMethod.Parameters.Length == overload.Parameters.Length - 1;
        var hasOneAdditionalOptionalCancellationTokenParameter = hasOneAdditionalParameter && overload.GetCancellationTokenFromParameters().IsNullable == true;
        var hasOneAdditionalRequiredCancellationTokenParameter = hasOneAdditionalParameter && overload.GetCancellationTokenFromParameters().IsNullable == false && hasCancellationTokenInContext;

        // We allow overloads to differ by providing a cancellationToken
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

        var isSurroundingMethod = overload.Equals(surroundingMethodDeclaration, SymbolEqualityComparer.Default);
        if (isSurroundingMethod)
        {
            return false;
        }

        var returnType = invokedMethod.ReturnType;
        var isVoidOverload = returnType.SpecialType == SpecialType.System_Void && overload.ReturnType.IsNonGenericTaskType();
        var isGenericTaskOverload =
            returnType.SpecialType != SpecialType.System_Void &&
            overload.ReturnType.IsGenericTaskType(out var wrappedType) &&
            wrappedType != default &&
            ( wrappedType.Equals(returnType, SymbolEqualityComparer.Default) || wrappedType.TypeKind == TypeKind.TypeParameter );
        var isPlainTaskOverload = returnType.IsNonGenericTaskType() && overload.ReturnType.IsNonGenericTaskType();

        return isVoidOverload || isGenericTaskOverload || isPlainTaskOverload;
    }
}