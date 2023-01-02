using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThreadSleepInAsyncMethodAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ThreadSleepInAsyncMethod,
        "Synchronously sleeping a thread in an async method",
        "Synchronously sleeping thread in an async method",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS032-ThreadSleepInAsyncMethod.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var threadSleepSymbols = context.Compilation.GetTypeByMetadataName("System.Threading.Thread")?.GetMembers("Sleep").OfType<IMethodSymbol>().ToArray();

            context.RegisterSymbolStartAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;
                if (( method.IsAsync || method.ReturnType.IsTaskType() ) && method.Name != WellKnownMemberNames.TopLevelStatementsEntryPointMethodName)
                {
                    symbolContext.RegisterOperationAction(context => Analyze(context, (IInvocationOperation)context.Operation, threadSleepSymbols, method.IsAsync), OperationKind.Invocation);
                }

                symbolContext.RegisterOperationAction(context => AnalyzeLambda(context, threadSleepSymbols), OperationKind.AnonymousFunction);
            }, SymbolKind.Method);

            context.RegisterOperationAction(context => AnalyzeLocalFunction(context, threadSleepSymbols), OperationKind.LocalFunction);
        });
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context, IMethodSymbol[]? threadSleepSymbols)
    {
        var localFunction = (ILocalFunctionOperation)context.Operation;
        if (localFunction.Symbol.IsAsync || localFunction.Symbol.ReturnType.IsTaskType())
        {
            // An ugly approach to only take the operations that are part of the current block
            // This way we avoid triggering the diagnostic multiple times when there are nested local functions
            foreach (var invocation in localFunction.Descendants().Skip(1).TakeWhile(d => d is not IBlockOperation).OfType<IInvocationOperation>())
            {
                Analyze(context, invocation, threadSleepSymbols, localFunction.Symbol.IsAsync);
            }
        }
    }

    private static void AnalyzeLambda(OperationAnalysisContext context, IMethodSymbol[]? threadSleepSymbols)
    {
        var lambda = (IAnonymousFunctionOperation)context.Operation;
        if (!lambda.Symbol.IsAsync)
        {
            return;
        }

        foreach (var invocation in lambda.Descendants().OfType<IInvocationOperation>())
        {
            Analyze(context, invocation, threadSleepSymbols, lambda.Symbol.IsAsync);
        }
    }

    private static void Analyze(OperationAnalysisContext context, IInvocationOperation invocation, IMethodSymbol[]? threadSleepSymbols, bool isAsync)
    {
        if (!threadSleepSymbols.Any(symbol => invocation.TargetMethod.Equals(symbol, SymbolEqualityComparer.Default)))
        {
            return;
        }

        if (invocation.Ancestors().Any(a => a is IAnonymousFunctionOperation { Symbol.IsAsync: false }))
        {
            return;
        }

        var dic = ImmutableDictionary.CreateBuilder<string, string?>();
        dic.Add("isAsync", isAsync.ToString());
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), dic.ToImmutable()));
    }
}