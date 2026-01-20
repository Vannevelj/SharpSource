using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ImmutableCollectionCreatedIncorrectlyAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ImmutableCollectionCreatedIncorrectly,
        "ImmutableArray is being created using 'new' instead of the Create method",
        "ImmutableArray should be created using ImmutableArray.Create<{0}>() instead of new ImmutableArray<{0}>()",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS061-ImmutableCollectionCreatedIncorrectly.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var immutableArrayType = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableArray`1");
            if (immutableArrayType is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(context => Analyze(context, immutableArrayType), OperationKind.ObjectCreation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol immutableArrayType)
    {
        // TODO: Implement analysis logic
    }
}