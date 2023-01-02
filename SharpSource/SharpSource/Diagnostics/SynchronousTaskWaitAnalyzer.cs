using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SynchronousTaskWaitAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.SynchronousTaskWait,
        "Asynchronously await tasks instead of blocking them",
        "Asynchronously wait for task completion using await instead",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS035-SynchronousTaskWait.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var taskWaitSymbols = compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")?.GetMembers("Wait").OfType<IMethodSymbol>().ToArray();

            compilationContext.RegisterSymbolStartAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;
                var isAsync = method.IsAsync || method.Name == WellKnownMemberNames.TopLevelStatementsEntryPointMethodName;
                if (isAsync || method.ReturnType.IsTaskType())
                {
                    symbolContext.RegisterOperationAction(context => Analyze(context, (IInvocationOperation)context.Operation, taskWaitSymbols), OperationKind.Invocation);
                }
            }, SymbolKind.Method);
        });
    }

    private static void Analyze(OperationAnalysisContext context, IInvocationOperation invocation, IMethodSymbol[]? taskWaitSymbols)
    {
        if (taskWaitSymbols is null || !taskWaitSymbols.Any(s => invocation.TargetMethod.Equals(s, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("numberOfArguments", invocation.Arguments.Length.ToString());

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable()));
    }
}