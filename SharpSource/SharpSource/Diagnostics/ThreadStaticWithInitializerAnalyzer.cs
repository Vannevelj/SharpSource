using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThreadStaticWithInitializerAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ThreadStaticWithInitializer,
        "A field is marked as [ThreadStatic] so it cannot contain an initializer. The field initializer is only executed for the first thread.",
        "{0} is marked as [ThreadStatic] so it cannot contain an initializer",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS052-ThreadStaticWithInitializer.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var threadStaticSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.ThreadStaticAttribute");
            if (threadStaticSymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => Analyze(context, threadStaticSymbol), OperationKind.FieldInitializer);
            }
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol threadStaticSymbol)
    {
        var initializer = (IFieldInitializerOperation)context.Operation;
        foreach (var field in initializer.InitializedFields)
        {
            if (field.GetAttributes().Any(a => threadStaticSymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, initializer.Syntax.GetLocation(), field.Name));
            }            
        }
    }
}