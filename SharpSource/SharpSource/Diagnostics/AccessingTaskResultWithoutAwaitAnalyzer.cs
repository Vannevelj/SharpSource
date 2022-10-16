using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AccessingTaskResultWithoutAwaitAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.AccessingTaskResultWithoutAwait,
        "Use await to get the result of an asynchronous operation",
        "Use await to get the result of a Task.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS034-AccessingTaskResultWithoutAwait.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        var invokedSymbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
        if (invokedSymbol == null)
        {
            return;
        }

        var isAsyncContext = context.Node.FirstAncestorOrSelfOfType(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.LocalFunctionStatement,
            SyntaxKind.ParenthesizedLambdaExpression) switch
        {
            MethodDeclarationSyntax method => method.Modifiers.ContainsAny(SyntaxKind.AsyncKeyword),
            LocalFunctionStatementSyntax local => local.Modifiers.ContainsAny(SyntaxKind.AsyncKeyword),
            ParenthesizedLambdaExpressionSyntax lambda => lambda.AsyncKeyword != default,
            _ => false
        };

        if (!isAsyncContext)
        {
            return;
        }

        if (invokedSymbol.Name == "Result" && invokedSymbol.ContainingType.IsTaskType())
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation()));
        }
    }
}