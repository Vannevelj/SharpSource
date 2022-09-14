using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LinqTraversalBeforeFilterAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering if performed first?";
    private static readonly string Title =
        "An IEnumerable extension method was used to traverse the collection and is subsequently filtered using Where()." +
        "If the Where() filter is executed first, the traversal will have to iterate over fewer items which will result in better performance.";

    private static readonly HashSet<string> TraversalOperations = new(){
        "OrderBy", "OrderByDescending", "Chunk", "Reverse", "Take", "TakeLast", "TakeWhile"
    };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LinqTraversalBeforeFilter,
        Title,
        Message,
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS047-LinqTraversalBeforeFilter.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var expression = (MemberAccessExpressionSyntax)context.Node;
        if (expression.Expression is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        var firstInvokedFunctionSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol;
        var firstInvokedFunction = firstInvokedFunctionSymbol?.Name;
        var secondInvokedFunction = expression.Name.Identifier.ValueText;
        if (firstInvokedFunction == default || secondInvokedFunction == default)
        {
            return;
        }

        if (secondInvokedFunction == "Where" && TraversalOperations.Contains(firstInvokedFunction))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation()));
        }
    }
}