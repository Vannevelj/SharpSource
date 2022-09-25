using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterAssignedInConstructorAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ParameterAssignedInConstructor,
        "A parameter was assigned in a constructor",
        "Suspicious assignment of parameter {0} in constructor of {1}",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS050-ParameterAssignedInConstructor.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ConstructorDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

        if (constructorDeclaration.ParameterList.Parameters is not { Count: >= 1 })
        {
            return;
        }

        var parameterSymbols = constructorDeclaration.ParameterList.Parameters.Select(param => context.SemanticModel.GetDeclaredSymbol(param));

        foreach (var statement in constructorDeclaration.GetStatements())
        {
            var assignment = statement switch
            {
                AssignmentExpressionSyntax assignmentExpression => assignmentExpression,
                ExpressionStatementSyntax expressionStatement when expressionStatement.Expression is AssignmentExpressionSyntax assignmentExpression => assignmentExpression,
                _ => default
            };

            if (assignment == default)
            {
                continue;
            }

            var leftAssignmentSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
            if (leftAssignmentSymbol == default)
            {
                continue;
            }

            var correspondingParameter = parameterSymbols.FirstOrDefault(parameterSymbol => leftAssignmentSymbol.Equals(parameterSymbol, SymbolEqualityComparer.Default));
            if (correspondingParameter == default)
            {
                continue;
            }

            if (correspondingParameter.RefKind is RefKind.Out or RefKind.Ref)
            {
                continue;
            }

            if (assignment.Right is not IdentifierNameSyntax)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Left.GetLocation(), leftAssignmentSymbol.Name, correspondingParameter?.ContainingType.Name));
        }
    }
}