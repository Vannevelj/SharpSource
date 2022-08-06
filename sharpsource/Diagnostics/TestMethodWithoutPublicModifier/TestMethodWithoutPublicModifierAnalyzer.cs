using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.TestMethodWithoutPublicModifier
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestMethodWithoutPublicModifierAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.TestsCategory;
        private static readonly string Message = Resources.TestMethodWithoutPublicModifierAnalyzerMessage;
        private static readonly string Title = Resources.TestMethodWithoutPublicModifierAnalyzerTitle;

        public static DiagnosticDescriptor Rule
            => new DiagnosticDescriptor(DiagnosticId.TestMethodWithoutPublicModifier, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            if (method.HasTestAttribute() && !method.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(),
                    method.Identifier.Text));
            }
        }
    }
}