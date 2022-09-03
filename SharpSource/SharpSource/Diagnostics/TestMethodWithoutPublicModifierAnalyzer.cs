using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodWithoutPublicModifierAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Test method \"{0}\" is not public.";
    private static readonly string Title = "Verifies whether a test method has the public modifier.";

    public static DiagnosticDescriptor Rule
        => new(DiagnosticId.TestMethodWithoutPublicModifier, Title, Message, Categories.Correctness, DiagnosticSeverity.Warning, true);

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