using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AccessingTaskResultWithoutAwaitAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.AccessingTaskResultWithoutAwait,
        "Use await to get the result of an asynchronous operation",
        "Use await to get the result of a Task.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS034-AccessingTaskResultWithoutAwait.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var resultProperties = ImmutableArray.Create(
                context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")?.GetMembers("Result").OfType<IPropertySymbol>().FirstOrDefault(),
                context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1")?.GetMembers("Result").OfType<IPropertySymbol>().FirstOrDefault(),
                context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask")?.GetMembers("Result").OfType<IPropertySymbol>().FirstOrDefault(),
                context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1")?.GetMembers("Result").OfType<IPropertySymbol>().FirstOrDefault()
            );

            context.RegisterOperationAction(context => AnalyzePropertyReference(context, resultProperties), OperationKind.PropertyReference);
        });
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context, ImmutableArray<IPropertySymbol?> resultProperties)
    {
        var operation = context.Operation;
        if (!resultProperties.Contains(((IPropertyReferenceOperation)operation).Property.OriginalDefinition, SymbolEqualityComparer.Default))
        {
            return;
        }

        var isAsyncContext = (context.ContainingSymbol as IMethodSymbol)?.IsAsync ?? false;
        while (operation is not null)
        {
            if (operation is IAnonymousFunctionOperation anonymousFunctionOperation)
            {
                isAsyncContext = anonymousFunctionOperation.Symbol.IsAsync;
                break;
            }
            else if (operation is ILocalFunctionOperation localFunctionOperation)
            {
                isAsyncContext = localFunctionOperation.Symbol.IsAsync;
                break;
            }
            else if (operation.Kind == OperationKind.MethodBody)
            {
                break;
            }

            operation = operation.Parent;
        }

        if (isAsyncContext)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation()));
        }
    }
}