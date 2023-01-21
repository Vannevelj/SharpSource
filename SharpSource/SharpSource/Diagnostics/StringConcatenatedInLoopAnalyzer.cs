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
            var surroundingLoop = assignment.Ancestors().OfType<ILoopOperation>().FirstOrDefault();
            if (surroundingLoop is null)
            {
                return;
            }

            if (assignment.Type is not { SpecialType: SpecialType.System_String })
            {
                return;
            }

            if (assignment.Target is ILocalReferenceOperation localRef &&
                surroundingLoop.Body is IBlockOperation body &&
                body.Locals.Any(s => s.Equals(localRef.Local, SymbolEqualityComparer.Default)))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.Syntax.GetLocation()));
        }, OperationKind.CompoundAssignment, OperationKind.SimpleAssignment);
    }
}