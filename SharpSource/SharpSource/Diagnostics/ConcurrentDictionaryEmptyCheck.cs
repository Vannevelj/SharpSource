using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConcurrentDictionaryEmptyCheckAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ConcurrentDictionaryEmptyCheck,
        "A ConcurrentDictionary is checked for emptiness without using .IsEmpty",
        "Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS060-ConcurrentDictionaryEmptyCheck.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var concurrentDictionarySymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Concurrent.ConcurrentDictionary`2");
            var countPropertySymbol = concurrentDictionarySymbol?.GetMembers("Count").OfType<IPropertySymbol>().FirstOrDefault();
            if (countPropertySymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => AnalyzeCountPropertyReference(context, countPropertySymbol), OperationKind.PropertyReference);
            }

            var enumerableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            var enumerableAnySymbol = enumerableSymbol?.GetMembers("Any").OfType<IMethodSymbol>().FirstOrDefault();
            if (enumerableAnySymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => AnalyzeAnyMethodReference(context, enumerableAnySymbol), OperationKind.Invocation);
            }

            var enumerableCountSymbol = enumerableSymbol?.GetMembers("Count").OfType<IMethodSymbol>().FirstOrDefault();
            if (enumerableCountSymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => AnalyzeCountMethodReference(context, enumerableCountSymbol), OperationKind.Invocation);
            }
        });
    }

    private static void AnalyzeCountPropertyReference(OperationAnalysisContext context, IPropertySymbol countProperty)
    {
        var propertyReference = (IPropertyReferenceOperation)context.Operation;

        if (countProperty.Equals(propertyReference.Property.OriginalDefinition, SymbolEqualityComparer.Default) &&
            propertyReference.Parent is IBinaryOperation binaryOperation)
        {
            var rightOperandConstant = binaryOperation.RightOperand.SemanticModel?.GetConstantValue(binaryOperation.RightOperand.Syntax);
            var leftOperandConstant = binaryOperation.LeftOperand.SemanticModel?.GetConstantValue(binaryOperation.LeftOperand.Syntax);
            var isRightOperandZero = rightOperandConstant is { HasValue: true, Value: 0 };
            if (rightOperandConstant is { HasValue: true, Value: 0 } || leftOperandConstant is { HasValue: true, Value: 0 })
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("isBinaryCheck", "true");
                properties.Add("binaryOperandOfInterest", isRightOperandZero ? "left" : "right");

                var mustInvert =
                    ( isRightOperandZero && binaryOperation.OperatorKind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.GreaterThan ) ||
                    ( !isRightOperandZero && binaryOperation.OperatorKind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.LessThan );
                properties.Add("mustInvert", mustInvert ? "true" : "false");

                context.ReportDiagnostic(Diagnostic.Create(Rule, binaryOperation.Syntax.GetLocation(), properties.ToImmutable()));
            }
        }
    }

    private static void AnalyzeAnyMethodReference(OperationAnalysisContext context, IMethodSymbol anyMethod)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.TargetMethod.OriginalDefinition.Equals(anyMethod, SymbolEqualityComparer.Default))
        {
            var properties = ImmutableDictionary.CreateBuilder<string, string?>();
            properties.Add("isBinaryCheck", "false");
            properties.Add("mustInvert", "true");

            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable()));
        }
    }

    private static void AnalyzeCountMethodReference(OperationAnalysisContext context, IMethodSymbol countMethod)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.TargetMethod.OriginalDefinition.Equals(countMethod, SymbolEqualityComparer.Default) &&
            invocation.Parent is IBinaryOperation binaryOperation)
        {
            var rightOperandConstant = binaryOperation.RightOperand.SemanticModel?.GetConstantValue(binaryOperation.RightOperand.Syntax);
            var leftOperandConstant = binaryOperation.LeftOperand.SemanticModel?.GetConstantValue(binaryOperation.LeftOperand.Syntax);
            var isRightOperandZero = rightOperandConstant is { HasValue: true, Value: 0 };
            if (rightOperandConstant is { HasValue: true, Value: 0 } || leftOperandConstant is { HasValue: true, Value: 0 })
            {
                var properties = ImmutableDictionary.CreateBuilder<string, string?>();
                properties.Add("isBinaryCheck", "true");
                properties.Add("binaryOperandOfInterest", isRightOperandZero ? "left" : "right");

                var mustInvert =
                    ( isRightOperandZero && binaryOperation.OperatorKind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.GreaterThan ) ||
                    ( !isRightOperandZero && binaryOperation.OperatorKind is BinaryOperatorKind.NotEquals or BinaryOperatorKind.LessThan );
                properties.Add("mustInvert", mustInvert ? "true" : "false");

                context.ReportDiagnostic(Diagnostic.Create(Rule, binaryOperation.Syntax.GetLocation(), properties.ToImmutable()));
            }
        }
    }
}