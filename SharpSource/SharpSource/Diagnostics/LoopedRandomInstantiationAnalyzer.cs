using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoopedRandomInstantiationAnalyzer : DiagnosticAnalyzer
{
    private readonly SyntaxKind[] _loopTypes = { SyntaxKind.ForEachStatement, SyntaxKind.ForStatement, SyntaxKind.WhileStatement, SyntaxKind.DoStatement };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LoopedRandomInstantiation,
        "An instance of type System.Random is created in a loop.",
        "Variable {0} of type System.Random is instantiated in a loop.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS009-LoopedRandomInstantiation.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.VariableDeclaration);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var variableDeclaration = (VariableDeclarationSyntax)context.Node;

        var type = variableDeclaration.Type;
        if (type == null)
        {
            return;
        }

        var typeInfo = context.SemanticModel.GetTypeInfo(type).Type;

        if (typeInfo?.OriginalDefinition.ContainingNamespace == null ||
            typeInfo.OriginalDefinition.ContainingNamespace.Name != nameof(System) ||
            typeInfo.Name != nameof(Random))
        {
            return;
        }

        SyntaxNode? currentNode = variableDeclaration;
        while (currentNode is not null && !currentNode.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration))
        {
            if (_loopTypes.Contains(currentNode.Kind()))
            {
                foreach (var declarator in variableDeclaration.Variables)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.GetLocation(), declarator.Identifier.Text));
                }
                return;
            }

            currentNode = currentNode.Parent;
        }
    }
}