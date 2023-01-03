using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedResultOnImmutableObjectAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.UnusedResultOnImmutableObject,
        "The result of an operation on an immutable object is unused",
        "The result of an operation on an immutable object is unused",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS040-UnusedResultOnImmutableObject.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var stringSymbol = compilationContext.Compilation.GetSpecialType(SpecialType.System_String);
            var allowedInvocations = stringSymbol.GetMembers("CopyTo").Concat(stringSymbol.GetMembers("TryCopyTo")).OfType<IMethodSymbol>().ToArray();

            compilationContext.RegisterOperationAction(context => Analyze(context, allowedInvocations), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, IMethodSymbol[] allowedInvocations)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (invocation.Instance?.Type?.SpecialType is not SpecialType.System_String)
        {
            return;
        }

        if (allowedInvocations.Any(i => i.Equals(invocation.TargetMethod, SymbolEqualityComparer.Default)))
        {
            return;
        }

        if (invocation.TargetMethod.IsExtensionMethod && !invocation.TargetMethod.IsDefinedInSystemAssembly())
        {
            return;
        }

        if (invocation.Syntax.Parent is ExpressionStatementSyntax expressionStatement &&
            expressionStatement.Parent is BlockSyntax or GlobalStatementSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
        }
    }
}