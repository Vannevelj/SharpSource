using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedResultOnImmutableObjectAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "The result of an operation on an immutable object is unused";
    private static readonly string Title = "The result of an operation on an immutable object is unused";

    public static DiagnosticDescriptor Rule
        => new(DiagnosticId.UnusedResultOnImmutableObject, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var typeBeingAccessed = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (typeBeingAccessed == null || typeBeingAccessed.SpecialType != SpecialType.System_String)
        {
            return;
        }

        var isBeingAssigned = invocation.FirstAncestorOfType(
            SyntaxKind.Block,
            SyntaxKind.GlobalStatement,
            SyntaxKind.VariableDeclarator,
            SyntaxKind.IfStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement);

        switch (isBeingAssigned?.Kind())
        {
            case SyntaxKind.Block:
            case SyntaxKind.GlobalStatement:
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation())); return;
        }
    }
}