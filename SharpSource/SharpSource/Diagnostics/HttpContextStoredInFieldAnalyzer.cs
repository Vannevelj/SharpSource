using System.Collections.Immutable;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HttpContextStoredInFieldAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "HttpContext was stored in a field. Use IHttpContextAccessor instead";
    private static readonly string Title = "HttpContext was stored in a field";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.HttpContextStoredInField, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetSymbolInfo(fieldDeclaration.Declaration?.Type).Symbol;

        if (symbol?.Name == "HttpContext" && symbol.IsDefinedInSystemAssembly())
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}