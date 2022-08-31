using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncMethodWithVoidReturnTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Method {0} is marked as async but has a void return type";
    private static readonly string Title = "Async methods should return a Task to make them awaitable";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.AsyncMethodWithVoidReturnType, Title, Message, Categories.Async, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var (returnType, modifiers, parameterList, identifier) = context.Node switch
        {
            MethodDeclarationSyntax method => (method.ReturnType, method.Modifiers, method.ParameterList, method.Identifier),
            LocalFunctionStatementSyntax local => (local.ReturnType, local.Modifiers, local.ParameterList, local.Identifier),
            _ => throw new NotSupportedException($"Unexpected node: {context.Node.GetType().Name}")
        };

        // Method has to return void
        var returnTypeInfo = context.SemanticModel.GetTypeInfo(returnType).Type;
        if (returnTypeInfo == null || returnTypeInfo.SpecialType != SpecialType.System_Void)
        {
            return;
        }

        if (modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return;
        }

        if (!modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
        {
            return;
        }

        // Event handlers can only have a void return type
        if (parameterList?.Parameters.Count == 2)
        {
            var parameters = parameterList.Parameters;
            var firstArgumentType = context.SemanticModel.GetTypeInfo(parameters[0].Type);
            var isFirstArgumentObject = firstArgumentType.Type != null &&
                                        firstArgumentType.Type.SpecialType == SpecialType.System_Object;


            var secondArgumentType = context.SemanticModel.GetTypeInfo(parameters[1].Type);
            var isSecondArgumentEventArgs = secondArgumentType.Type != null &&
                                            secondArgumentType.Type.InheritsFrom(typeof(EventArgs));

            if (isFirstArgumentObject && isSecondArgumentEventArgs)
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, returnType.GetLocation(), identifier.ValueText));
    }
}