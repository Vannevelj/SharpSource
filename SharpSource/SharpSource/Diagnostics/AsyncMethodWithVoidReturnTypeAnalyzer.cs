using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncMethodWithVoidReturnTypeAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.AsyncMethodWithVoidReturnType,
        "Async methods should return a Task to make them awaitable",
        "Method {0} is marked as async but has a void return type",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS001-AsyncMethodWithVoidReturnType.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var eventArgsSymbol = context.Compilation.GetTypeByMetadataName("System.EventArgs");
            if (eventArgsSymbol is not null)
            {
                context.RegisterSymbolAction(context => AnalyzeMethod(context, eventArgsSymbol), SymbolKind.Method);
                context.RegisterOperationAction(context => AnalyzeLocalFunction(context, eventArgsSymbol), OperationKind.LocalFunction);
            }
        });
    }

    private static void AnalyzeLocalFunction(OperationAnalysisContext context, INamedTypeSymbol eventArgsSymbol)
    {
        var localFunctionOperation = (ILocalFunctionOperation)context.Operation;
        var method = localFunctionOperation.Symbol;
        if (ShouldReportDiagnostic(method, eventArgsSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context, INamedTypeSymbol eventArgsSymbol)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (ShouldReportDiagnostic(method, eventArgsSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
        }
    }

    private static bool ShouldReportDiagnostic(IMethodSymbol method, INamedTypeSymbol eventArgsSymbol)
    {
        if (method.ReturnType.SpecialType != SpecialType.System_Void)
        {
            return false;
        }

        if (method.PartialDefinitionPart is not null || method.PartialImplementationPart is not null)
        {
            return false;
        }

        if (!method.IsAsync)
        {
            return false;
        }

        // We don't trigger for implementations of interface/abstract definitions since they can't be unless the definition changes
        if (method.IsOverride || method.IsInterfaceImplementation())
        {
            return false;
        }

        // Event handlers can only have a void return type
        if (method.Parameters.Length == 2)
        {
            var isFirstParameterObject = method.Parameters[0].Type.SpecialType == SpecialType.System_Object;
            var isSecondParameterEventArgs = method.Parameters[1].Type.InheritsFrom(eventArgsSymbol);

            if (isFirstParameterObject && isSecondParameterEventArgs)
            {
                return false;
            }
        }

        return true;
    }
}