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

            var isEligible = assignment switch
            {
                ICompoundAssignmentOperation { OperatorKind: BinaryOperatorKind.Add } => true,
                ISimpleAssignmentOperation { Value: IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } } => true,
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
                var instanceToFind = GetLocal(assignment.Target);
                var isReferencingLocal = surroundingLoop.Locals.Concat(body.Locals).Any(s => s.Equals(instanceToFind, SymbolEqualityComparer.Default));
                if (isReferencingLocal)
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Syntax.GetLocation()));
        }, OperationKind.CompoundAssignment, OperationKind.SimpleAssignment);
    }

    private static ISymbol? GetLocal(IOperation? operation) => operation switch
    {
        ILocalReferenceOperation localRef => localRef.Local,
        IPropertyReferenceOperation propRef => GetLocal(propRef.Instance),
        IFieldReferenceOperation fieldRef => GetLocal(fieldRef.Instance),
        _ => default
    };
}