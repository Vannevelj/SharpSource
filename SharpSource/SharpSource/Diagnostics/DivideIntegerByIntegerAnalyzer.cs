using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DivideIntegerByIntegerAnalyzer : DiagnosticAnalyzer
{
    private static readonly SpecialType[] IntegerTypes =
    {
        SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64,
        SpecialType.System_SByte, SpecialType.System_UInt16, SpecialType.System_UInt32, SpecialType.System_UInt64
    };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.DivideIntegerByInteger,
        "The operands of a divisive expression are both integers and result in an implicit rounding.",
        "The operands in the divisive expression {0} are both integers and result in an implicit rounding.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS003-DivideIntegerByInteger.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterOperationAction(AnalyzeBinaryOperation, OperationKind.Binary);
    }

    private static void AnalyzeBinaryOperation(OperationAnalysisContext context)
    {
        var binaryOperation = (IBinaryOperation)context.Operation;
        if (binaryOperation.OperatorKind != BinaryOperatorKind.Divide)
        {
            return;
        }

        var leftType = binaryOperation.LeftOperand.Type;
        if (leftType == null)
        {
            return;
        }

        if (IntegerTypes.Contains(leftType.SpecialType))
        {
            var rightType = binaryOperation.RightOperand.Type;
            if (rightType == null)
            {
                return;
            }

            if (IntegerTypes.Contains(rightType.SpecialType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, binaryOperation.Syntax.GetLocation(), binaryOperation.Syntax.ToString()));
            }
        }
    }
}