using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ThreadStaticWithInitializerAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ThreadStaticWithInitializer,
        "A field is marked as [ThreadStatic] so it cannot contain an initializer. The field initializer is only executed for the first thread.",
        "{0} is marked as [ThreadStatic] so it cannot contain an initializer",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS052-ThreadStaticWithInitializer.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.FieldDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var field = (FieldDeclarationSyntax) context.Node;
        var hasThreadStaticAttribute = field.AttributeLists.GetAttributesOfType(typeof(System.ThreadStaticAttribute), context.SemanticModel).Any();
        
        if (!hasThreadStaticAttribute)
        {
            return;
        }

        foreach (var variableDeclaration in field.Declaration.Variables.Where(v => v.Initializer is not null))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, variableDeclaration.Initializer?.GetLocation(), variableDeclaration.Identifier.ValueText));
        }
    }
}