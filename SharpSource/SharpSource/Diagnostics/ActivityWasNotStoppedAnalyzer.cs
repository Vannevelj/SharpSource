using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ActivityWasNotStoppedAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.ActivityWasNotStopped,
        "An Activity was created but not stopped",
        "Activity {0} was started but is not being stopped or disposed",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS062-ActivityWasNotStopped.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var activitySymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Diagnostics.Activity");
            var activitySourceSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Diagnostics.ActivitySource");
            if (activitySymbol is null || activitySourceSymbol is null)
            {
                return;
            }

            compilationContext.RegisterOperationBlockAction(c => AnalyzeOperationBlock(c, activitySymbol, activitySourceSymbol));
        });
    }

    private static void AnalyzeOperationBlock(OperationBlockAnalysisContext context, INamedTypeSymbol activitySymbol, INamedTypeSymbol activitySourceSymbol)
    {
        foreach (var operationBlock in context.OperationBlocks)
        {
            var startActivityInvocations = new List<(IInvocationOperation Invocation, ILocalSymbol? LocalSymbol, bool IsEscapedDirectly)>();
            var stoppedOrDisposedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            var escapedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

            // Walk the operation tree to collect data
            foreach (var operation in operationBlock.DescendantsAndSelf())
            {
                // Find StartActivity invocations
                if (operation is IInvocationOperation invocation &&
                    IsStartActivityMethod(invocation, activitySourceSymbol))
                {
                    var isUsing = IsInUsingContext(invocation);
                    
                    if (!isUsing)
                    {
                        var (localSymbol, isEscaped) = GetAssignmentTarget(invocation);
                        startActivityInvocations.Add((invocation, localSymbol, isEscaped));
                    }
                }

                // Find Stop() or Dispose() calls
                if (operation is IInvocationOperation methodInvocation &&
                    methodInvocation.Instance is ILocalReferenceOperation localRef &&
                    IsActivityType(localRef.Type, activitySymbol) &&
                    IsStopOrDisposeMethod(methodInvocation))
                {
                    stoppedOrDisposedLocals.Add(localRef.Local);
                }

                // Find conditional access Stop() or Dispose() calls (activity?.Stop())
                if (operation is IConditionalAccessOperation conditionalAccess &&
                    conditionalAccess.Operation is ILocalReferenceOperation condLocalRef &&
                    IsActivityType(condLocalRef.Type, activitySymbol) &&
                    conditionalAccess.WhenNotNull is IInvocationOperation condMethodInvocation &&
                    IsStopOrDisposeMethod(condMethodInvocation))
                {
                    stoppedOrDisposedLocals.Add(condLocalRef.Local);
                }

                // Track escaped locals (returned, passed as argument, assigned to field/property/out param)
                TrackEscapedLocals(operation, activitySymbol, escapedLocals);
            }

            // Report diagnostics for activities that were not properly stopped
            foreach (var (invocation, localSymbol, isEscapedDirectly) in startActivityInvocations)
            {
                // If the StartActivity was directly assigned to a field/property/parameter, it's escaped
                if (isEscapedDirectly)
                {
                    continue;
                }

                if (localSymbol is null)
                {
                    // Activity wasn't assigned to any variable - report diagnostic
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), ""));
                    continue;
                }

                if (stoppedOrDisposedLocals.Contains(localSymbol))
                {
                    continue;
                }

                if (escapedLocals.Contains(localSymbol))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), localSymbol.Name));
            }
        }
    }

    private static bool IsStartActivityMethod(IInvocationOperation invocation, INamedTypeSymbol activitySourceSymbol)
    {
        if (invocation.TargetMethod is null)
        {
            return false;
        }

        return invocation.TargetMethod.Name == "StartActivity" &&
               SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, activitySourceSymbol);
    }

    private static bool IsActivityType(ITypeSymbol? type, INamedTypeSymbol activitySymbol)
    {
        if (type is null)
        {
            return false;
        }

        // Handle nullable reference types (Activity?)
        var underlyingType = type.WithNullableAnnotation(NullableAnnotation.None);

        return SymbolEqualityComparer.Default.Equals(underlyingType, activitySymbol);
    }

    private static bool IsStopOrDisposeMethod(IInvocationOperation invocation)
    {
        var methodName = invocation.TargetMethod?.Name;
        return methodName is "Stop" or "Dispose";
    }

    private static (ILocalSymbol? LocalSymbol, bool IsEscapedDirectly) GetAssignmentTarget(IInvocationOperation invocation)
    {
        // Check if the invocation is part of a variable declaration
        var parent = invocation.Parent;

        // Handle direct assignment: var activity = source.StartActivity(...)
        if (parent is IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator })
        {
            return (declarator.Symbol, false);
        }

        // Handle simple assignment: activity = source.StartActivity(...)
        if (parent is ISimpleAssignmentOperation assignment)
        {
            if (assignment.Target is ILocalReferenceOperation localRef)
            {
                return (localRef.Local, false);
            }

            // Assigned to parameter (out param case)
            if (assignment.Target is IParameterReferenceOperation)
            {
                return (null, true);
            }

            // Assigned to field
            if (assignment.Target is IFieldReferenceOperation)
            {
                return (null, true);
            }

            // Assigned to property
            if (assignment.Target is IPropertyReferenceOperation)
            {
                return (null, true);
            }
        }

        return (null, false);
    }

    private static bool IsInUsingContext(IInvocationOperation invocation)
    {
        var current = invocation.Parent;
        while (current is not null)
        {
            if (current is IUsingOperation or IUsingDeclarationOperation)
            {
                return true;
            }

            if (current is IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Parent: IVariableDeclarationOperation { Parent: IUsingDeclarationOperation or IUsingOperation } } })
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    private static void TrackEscapedLocals(IOperation operation, INamedTypeSymbol activitySymbol, HashSet<ILocalSymbol> escapedLocals)
    {
        // Returned from method
        if (operation is IReturnOperation returnOp)
        {
            var localRef = ExtractLocalReference(returnOp.ReturnedValue);
            if (localRef is not null && IsActivityType(localRef.Type, activitySymbol))
            {
                escapedLocals.Add(localRef.Local);
            }
        }

        // Passed as argument to another method
        if (operation is IArgumentOperation argument)
        {
            var localRef = ExtractLocalReference(argument.Value);
            if (localRef is not null && IsActivityType(localRef.Type, activitySymbol))
            {
                escapedLocals.Add(localRef.Local);
            }
        }

        // Assigned to field
        if (operation is ISimpleAssignmentOperation fieldAssignment &&
            fieldAssignment.Target is IFieldReferenceOperation)
        {
            var localRef = ExtractLocalReference(fieldAssignment.Value);
            if (localRef is not null && IsActivityType(localRef.Type, activitySymbol))
            {
                escapedLocals.Add(localRef.Local);
            }
        }

        // Assigned to property
        if (operation is ISimpleAssignmentOperation propAssignment &&
            propAssignment.Target is IPropertyReferenceOperation)
        {
            var localRef = ExtractLocalReference(propAssignment.Value);
            if (localRef is not null && IsActivityType(localRef.Type, activitySymbol))
            {
                escapedLocals.Add(localRef.Local);
            }
        }

        // Assigned to out parameter
        if (operation is ISimpleAssignmentOperation outAssignment &&
            outAssignment.Target is IParameterReferenceOperation { Parameter.RefKind: RefKind.Out })
        {
            var localRef = ExtractLocalReference(outAssignment.Value);
            if (localRef is not null && IsActivityType(localRef.Type, activitySymbol))
            {
                escapedLocals.Add(localRef.Local);
            }
        }
    }

    private static ILocalReferenceOperation? ExtractLocalReference(IOperation? operation)
    {
        return operation switch
        {
            ILocalReferenceOperation localRef => localRef,
            IConversionOperation conversion => ExtractLocalReference(conversion.Operand),
            _ => null
        };
    }
}
