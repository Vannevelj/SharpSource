using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DateTimeNowAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Use DateTime.UtcNow to get a consistent value";
    private static readonly string Title = "DateTime.Now was referenced";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.DateTimeNow, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

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

        if (context.SemanticModel.GetSymbolInfo(expression.Expression).Symbol is INamedTypeSymbol symbol &&
            symbol.SpecialType == SpecialType.System_DateTime &&
            expression.Name.Identifier.ValueText == "Now")
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }
}