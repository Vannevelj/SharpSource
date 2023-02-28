using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringConcatenatedInLoopAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StringConcatenatedInLoop,
        "A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead.",
        "A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead.",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS058-StringConcatenatedInLoop.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterOperationAction(context =>
        {
            var assignment = (IAssignmentOperation)context.Operation;
            var assignedSymbol = GetSymbol(assignment.Target);

            var isEligible = assignment switch
            {
                ICompoundAssignmentOperation { OperatorKind: BinaryOperatorKind.Add } => true,
                ISimpleAssignmentOperation { Value: IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binary } => BinaryOperationConcatenatesSymbol(binary, assignedSymbol),
                _ => false
            };

            if (!isEligible)
            {
                return;
            }

            if (assignment.Type is not { SpecialType: SpecialType.System_String })
            {
                return;
            }

            var surroundingLoop = assignment.Ancestors().OfType<ILoopOperation>().FirstOrDefault();
            if (surroundingLoop is null)
            {
                return;
            }

            var nestedInsideObjectCreation = assignment.Ancestors().OfType<IObjectCreationOperation>().FirstOrDefault() is not null;
            if (nestedInsideObjectCreation)
            {
                return;
            }

            if (surroundingLoop.Body is IBlockOperation body)
            {
                var isReferencingLocal = surroundingLoop.Locals.Concat(body.Locals).Any(s => s.Equals(assignedSymbol, SymbolEqualityComparer.Default));
                if (isReferencingLocal)
                {
                    return;
                }
            }

            if (IsAdjacentToLoopEscape(assignment))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Syntax.GetLocation()));
        }, OperationKind.CompoundAssignment, OperationKind.SimpleAssignment);
    }

    private static ISymbol? GetSymbol(IOperation? operation) => operation switch
    {
        ILocalReferenceOperation localRef => localRef.Local,
        IPropertyReferenceOperation { Property.IsIndexer: false } propRef => GetSymbol(propRef.Instance),
        IFieldReferenceOperation fieldRef => GetSymbol(fieldRef.Instance),
        _ => default
    };

    private static bool BinaryOperationConcatenatesSymbol(IBinaryOperation binary, ISymbol? targetSymbol)
    {
        if (targetSymbol is null)
        {
            return false;
        }

        static bool traverse(IOperation op, ISymbol targetSymbol)
        {
            if (op is IBinaryOperation nestedBinaryOp)
            {
                return traverse(nestedBinaryOp.LeftOperand, targetSymbol) || traverse(nestedBinaryOp.RightOperand, targetSymbol);
            }

            var symbol = GetSymbol(op);
            return SymbolEqualityComparer.Default.Equals(symbol, targetSymbol);
        }

        return traverse(binary, targetSymbol);
    }

    private static bool IsAdjacentToLoopEscape(IOperation operation)
    {
        var statementContext = operation.Ancestors().OfType<IExpressionStatementOperation>().FirstOrDefault().Parent;
        var nextOperation = statementContext?.ChildOperations.SkipWhile(op => op.DescendantsAndSelf().All(d => d != operation)).Skip(1).FirstOrDefault();
        if (nextOperation is null)
        {
            return false;
        }

        return nextOperation is IReturnOperation or IBranchOperation { BranchKind: BranchKind.Break };
    }
}