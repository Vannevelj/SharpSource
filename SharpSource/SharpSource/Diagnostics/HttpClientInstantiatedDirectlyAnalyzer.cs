using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpClientInstantiatedDirectlyAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.HttpClientInstantiatedDirectly,
        "HttpClient was instantiated directly",
        "HttpClient was instantiated directly. Use IHttpClientFactory instead",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS037-HttpClientInstantiatedDirectly.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterOperationAction(AnalyzeCreation, OperationKind.ObjectCreation);
    }

    private static void AnalyzeCreation(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;
        if (objectCreation is { Type: { Name: "HttpClient" } } && objectCreation.Type.IsDefinedInSystemAssembly())
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation()));
        }
    }
}