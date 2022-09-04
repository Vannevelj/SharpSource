using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MultipleFromBodyParametersAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Method {0} specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.";
    private static readonly string Title = "A method was defined with multiple [FromBody] parameters but ASP.NET only supports a single one.";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.MultipleFromBodyParameters, Title, Message, Categories.Correctness, DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var attributesOnParameters = methodDeclaration
            .ParameterList
            .Parameters
            .SelectMany(p => p.AttributeLists)
            .SelectMany(x => x.Attributes)
            .Select(a => context.SemanticModel.GetSymbolInfo(a.Name).Symbol?.ContainingSymbol)
            .Count(s => s?.MetadataName is "FromBody" or "FromBodyAttribute" && s.IsDefinedInSystemAssembly());

        if (attributesOnParameters > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.ValueText));
        }
    }
}