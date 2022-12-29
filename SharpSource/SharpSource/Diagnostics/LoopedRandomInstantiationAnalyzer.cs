using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoopedRandomInstantiationAnalyzer : DiagnosticAnalyzer
{
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

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var randomSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Random");
            if (randomSymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => Analyze(context, randomSymbol), OperationKind.VariableDeclarator);
            }
        });
    }

    private void Analyze(OperationAnalysisContext context, INamedTypeSymbol randomSymbol)
    {
        var declarator = (IVariableDeclaratorOperation)context.Operation;
        if (!randomSymbol.Equals(declarator.Symbol.Type, SymbolEqualityComparer.Default))
        {
            return;
        }

        IOperation? currentNode = declarator;
        while (currentNode is not null)
        {
            if (currentNode is ILoopOperation)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.Syntax.GetLocation(), declarator.Symbol.Name));
                return;
            }

            currentNode = currentNode.Parent;
        }
    }
}