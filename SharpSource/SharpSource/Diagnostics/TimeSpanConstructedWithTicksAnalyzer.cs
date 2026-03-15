using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TimeSpanConstructedWithTicksAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.TimeSpanConstructedWithTicks,
        "TimeSpan single-parameter constructor creates ticks, not seconds",
        "TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS068-TimeSpanConstructedWithTicks.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var timeSpanSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.TimeSpan");
            if (timeSpanSymbol is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(context => AnalyzeCreation(context, timeSpanSymbol), OperationKind.ObjectCreation);
        });
    }

    private static void AnalyzeCreation(OperationAnalysisContext context, INamedTypeSymbol timeSpanSymbol)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;
        if (!timeSpanSymbol.Equals(objectCreation.Type, SymbolEqualityComparer.Default))
        {
            return;
        }

        if (objectCreation.Arguments.Length != 1)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation()));
    }
}
