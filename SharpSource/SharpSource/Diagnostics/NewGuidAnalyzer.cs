using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NewGuidAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.NewGuid,
        "Attempted to create empty guid",
        "An empty guid was created in an ambiguous manner",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS010-NewGuid.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var guidSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Guid");
            if (guidSymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => AnalyzeCreation(context, guidSymbol), OperationKind.ObjectCreation);
            }
        });
    }

    private static void AnalyzeCreation(OperationAnalysisContext context, INamedTypeSymbol guidSymbol)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;
        if (guidSymbol.Equals(objectCreation.Type, SymbolEqualityComparer.Default) && objectCreation is { Arguments.IsEmpty: true })
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation()));
        }
    }
}