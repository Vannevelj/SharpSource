using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThreadSleepInAsyncMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Synchronously sleeping thread in an async method";
    private static readonly string Title = "Synchronously sleeping a thread in an async method";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.ThreadSleepInAsyncMethod, Title, Message, Categories.Correctness, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var isAccessingThreadDotSleep = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => IsAccessingThreadDotSleep(memberAccess.Name, context),
            IdentifierNameSyntax identifierName => IsAccessingThreadDotSleep(identifierName, context),
            _ => false,
        };

        if (!isAccessingThreadDotSleep)
        {
            return;
        }

        var (found, returnType, modifiers) = context.Node.FirstAncestorOfType(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.LocalFunctionStatement,
            SyntaxKind.ParenthesizedLambdaExpression) switch
        {
            MethodDeclarationSyntax method => (true, method.ReturnType, method.Modifiers),
            LocalFunctionStatementSyntax local => (true, local.ReturnType, local.Modifiers),
            _ => (false, default, default)
        };

        if (!found || returnType == default)
        {
            return;
        }

        var isAsync = modifiers.Contains(SyntaxKind.AsyncKeyword);
        var returnTypeInfo = context.SemanticModel.GetTypeInfo(returnType);
        var hasTaskReturnType = returnTypeInfo.Type?.IsTaskType();

        if (isAsync || hasTaskReturnType == true)
        {
            var dic = ImmutableDictionary.CreateBuilder<string, string?>();
            dic.Add("isAsync", isAsync.ToString());
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), dic.ToImmutable(), null));
        }
    }

    private bool IsAccessingThreadDotSleep(SimpleNameSyntax invokedFunction, SyntaxNodeAnalysisContext context)
    {
        var invokedSymbol = context.SemanticModel.GetSymbolInfo(invokedFunction).Symbol;
        return invokedSymbol is { ContainingType.Name: "Thread", Name: "Sleep" };
    }
}