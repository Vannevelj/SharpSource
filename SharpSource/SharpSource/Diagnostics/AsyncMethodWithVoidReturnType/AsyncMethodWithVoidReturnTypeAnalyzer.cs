using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodWithVoidReturnTypeAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.AsyncCategory;
        private static readonly string Message = Resources.AsyncMethodWithVoidReturnTypeMessage;
        private static readonly string Title = Resources.AsyncMethodWithVoidReturnTypeTitle;

        public static DiagnosticDescriptor Rule => new(DiagnosticId.AsyncMethodWithVoidReturnType, Title, Message, Category, Severity, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // Method has to return void
            var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType);
            if (returnType.Type == null || returnType.Type.SpecialType != SpecialType.System_Void)
            {
                return;
            }

            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
            {
                return;
            }

            if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
            {
                return;
            }

            // Event handlers can only have a void return type
            if (method.ParameterList?.Parameters.Count == 2)
            {
                var parameters = method.ParameterList.Parameters;
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

            context.ReportDiagnostic(Diagnostic.Create(Rule, method.ReturnType.GetLocation(), method.Identifier.ValueText));
        }
    }
}