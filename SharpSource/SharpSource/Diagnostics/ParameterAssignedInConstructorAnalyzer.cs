using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterOperationBlockStartAction(context =>
        {
            if (context.OwningSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor })
            {
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

                    if (assignment.Value.Syntax is not IdentifierNameSyntax)
                    {
                        return;
                    }

                    if (assignment.Value is { ConstantValue.HasValue: true })
                    {
                        return;
                    }

                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameterReference.Syntax.GetLocation(), parameterReference.Parameter.Name, parameterReference.Parameter.ContainingType.Name));
                }, OperationKind.SimpleAssignment);
            }
        });
    }
}