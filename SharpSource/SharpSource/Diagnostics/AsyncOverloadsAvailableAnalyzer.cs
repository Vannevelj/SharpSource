using System.Collections.Immutable;
using System.Linq;
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var cancellationTokenSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
            if (cancellationTokenSymbol is not null)
            {
                compilationContext.RegisterOperationBlockAction(context => Analyze(context, cancellationTokenSymbol));
            }
        });
    }

    private static void Analyze(OperationBlockAnalysisContext context, INamedTypeSymbol cancellationTokenSymbol)
    {
        if (context.OwningSymbol is not IMethodSymbol surroundingMethod)
        {
            return;
        }

        if (surroundingMethod is { IsAsync: false })
        {
            return;
        }

        var invocations = context.OperationBlocks.SelectMany(block => block.Descendants().OfType<IInvocationOperation>());

        foreach (var invocation in invocations)
        {
            var invokedMethodName = invocation.TargetMethod.Name;
            var invokedTypeName = invocation.TargetMethod.ContainingType.Name;

            var methodsInInvokedType = invocation.TargetMethod.ContainingType.GetMembers().OfType<IMethodSymbol>();
            var relevantOverloads = methodsInInvokedType.Where(x => x.Name == $"{invokedMethodName}Async");

            foreach (var overload in relevantOverloads)
            {
                if (IsIdenticalOverload(invocation.TargetMethod, overload, surroundingMethod))
                {
                    var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                    var (cancellationTokenName, cancellationTokenIsNullable) = surroundingMethod.GetCancellationTokenFromParameters();

                    var currentInvocationPassesCancellationToken = invocation.PassesThroughCancellationToken(cancellationTokenSymbol);
                    var newInvocationAcceptsCancellationToken = overload.GetCancellationTokenFromParameters() != default;

                    properties.Add("cancellationTokenName", cancellationTokenName);
                    properties.Add("cancellationTokenIsOptional", cancellationTokenIsNullable == true ? "true" : "false");

                    var shouldAddCancellationToken = cancellationTokenName != default && !currentInvocationPassesCancellationToken && newInvocationAcceptsCancellationToken;
                    properties.Add("shouldAddCancellationToken", shouldAddCancellationToken ? "true" : "false");
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), $"{invokedTypeName}.{invokedMethodName}"));
                }
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
         * If the current context does provide a cancellationtoken and the current invocation uses it but the overload doesn't accept it, we need to remove it
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