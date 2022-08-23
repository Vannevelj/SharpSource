using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DivideIntegerByIntegerAnalyzer : DiagnosticAnalyzer
{
    private static readonly SpecialType[] IntegerTypes =
    {
        SpecialType.System_Byte, SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64,
        SpecialType.System_SByte, SpecialType.System_UInt16, SpecialType.System_UInt32, SpecialType.System_UInt64
    };

    private static readonly string Message = "The operands in the divisive expression {0} are both integers and result in an implicit rounding.";
    private static readonly string Title = "The operands of a divisive expression are both integers and result in an implicit rounding.";

    public static DiagnosticDescriptor Rule =>
            new(DiagnosticId.DivideIntegerByInteger, Title, Message, Categories.Arithmetic, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.DivideExpression);
    }

    private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var divideExpression = (BinaryExpressionSyntax)context.Node;
        var leftType = context.SemanticModel.GetTypeInfo(divideExpression.Left).Type;
        if (leftType == null)
        {
            return;
        }

        if (IntegerTypes.Contains(leftType.SpecialType))
        {
            var rightType = context.SemanticModel.GetTypeInfo(divideExpression.Right).Type;
            if (rightType == null)
            {
                return;
            }

            if (IntegerTypes.Contains(rightType.SpecialType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, divideExpression.GetLocation(), divideExpression.ToString()));
            }
        }
    }
}