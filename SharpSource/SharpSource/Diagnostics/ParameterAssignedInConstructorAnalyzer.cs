using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterAssignedInConstructorAnalyzer : DiagnosticAnalyzer
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationBlockStartAction(context =>
        {
            if (context.OwningSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor })
            {
                return;
            }

            var suspiciousAssignments = new List<(IParameterReferenceOperation ParamRef, ISimpleAssignmentOperation Assignment)>();

            context.RegisterOperationAction(context =>
            {
                var assignment = (ISimpleAssignmentOperation)context.Operation;
                if (assignment.Target is not IParameterReferenceOperation parameterReference)
                {
                    return;
                }

                if (parameterReference.Parameter.RefKind is RefKind.Out or RefKind.Ref)
                {
                    return;
                }

                if (assignment.Value is not IMemberReferenceOperation { Member: IFieldSymbol or IPropertySymbol })
                {
                    return;
                }

                if (assignment.Value is { ConstantValue.HasValue: true })
                {
                    return;
                }

                suspiciousAssignments.Add((parameterReference, assignment));
            }, OperationKind.SimpleAssignment);

            context.RegisterOperationBlockEndAction(context =>
            {
                if (suspiciousAssignments.Count == 0)
                {
                    return;
                }

                // Collect parameters that are later stored into fields/properties (normalization pattern)
                var parametersStoredInMembers = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
                foreach (var block in context.OperationBlocks)
                {
                    foreach (var operation in block.Descendants())
                    {
                        if (operation is ISimpleAssignmentOperation
                            {
                                Target: IMemberReferenceOperation { Member: IFieldSymbol or IPropertySymbol },
                                Value: IParameterReferenceOperation storedParam
                            })
                        {
                            parametersStoredInMembers.Add(storedParam.Parameter);
                        }
                    }
                }

                foreach (var (paramRef, _) in suspiciousAssignments)
                {
                    if (!parametersStoredInMembers.Contains(paramRef.Parameter))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, paramRef.Syntax.GetLocation(), paramRef.Parameter.Name, paramRef.Parameter.ContainingType.Name));
                    }
                }
            });
        });
    }
}