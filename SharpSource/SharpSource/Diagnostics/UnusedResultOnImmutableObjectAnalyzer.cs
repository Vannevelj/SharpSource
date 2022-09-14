using System.Collections.Generic;
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
    private static readonly HashSet<string> AllowedInvocations = new() { "CopyTo", "TryCopyTo" };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.UnusedResultOnImmutableObject,
        "The result of an operation on an immutable object is unused",
        "The result of an operation on an immutable object is unused",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS040-UnusedResultOnImmutableObject.md");

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

        if (AllowedInvocations.Contains(memberAccess.Name.Identifier.ValueText))
        {
            return;
        }

        if (invocation.Parent is ExpressionStatementSyntax expressionStatement &&
            expressionStatement.Parent is BlockSyntax or GlobalStatementSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
        }
    }
}