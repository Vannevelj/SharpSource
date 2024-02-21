using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThrowNullAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ThrowNull,
        "Throwing null will always result in a runtime exception",
        "Throwing null will always result in a runtime exception",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS006-ThrowNull.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationAction(AnalyzeThrow, OperationKind.Throw);
    }

    private void AnalyzeThrow(OperationAnalysisContext context)
    {
        var throwOperation = (IThrowOperation)context.Operation;
        if (throwOperation.Exception is { ConstantValue: { HasValue: true, Value: null } })
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, throwOperation.Syntax.GetLocation()));
        }
    }
}