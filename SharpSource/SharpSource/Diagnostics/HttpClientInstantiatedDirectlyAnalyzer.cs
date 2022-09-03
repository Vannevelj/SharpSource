using System.Collections.Immutable;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpClientInstantiatedDirectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "HttpClient was instantiated directly. Use IHttpClientFactory instead";
    private static readonly string Title = "HttpClient was instantiated directly";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.HttpClientInstantiatedDirectly, Title, Message, Categories.Performance, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var expression = (ObjectCreationExpressionSyntax)context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo(expression.Type).Symbol;

        if (symbol?.Name == "HttpClient" && ( symbol.ContainingAssembly.Name == "mscorlib" || symbol.ContainingAssembly.Name == "System.Net.Http" ))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}