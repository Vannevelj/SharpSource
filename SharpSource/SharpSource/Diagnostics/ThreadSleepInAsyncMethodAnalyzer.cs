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
                var isAsync = method.IsAsync || method.Name == WellKnownMemberNames.TopLevelStatementsEntryPointMethodName;
                if (isAsync || method.ReturnType.IsTaskType())
                {
                    symbolContext.RegisterOperationAction(context => Analyze(context, (IInvocationOperation)context.Operation, threadSleepSymbols, isAsync), OperationKind.Invocation);
                }
            }, SymbolKind.Method);
        });
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