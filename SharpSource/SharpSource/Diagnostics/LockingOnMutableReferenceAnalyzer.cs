using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LockingOnMutableReferenceAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LockingOnMutableReference,
        "A lock was obtained on a mutable field which can lead to deadlocks when a new value is assigned. Mark the field as readonly to prevent re-assignment after a lock is taken.",
        "A lock was obtained on {0} but the field is mutable. This can lead to deadlocks when a new value is assigned.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS051-LockingOnMutableReference.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterOperationAction(AnalyzeLockOperation, OperationKind.Lock);
    }

    private static void AnalyzeLockOperation(OperationAnalysisContext context)
    {
        var lockOperation = (ILockOperation)context.Operation;
        var referencedSymbol = ( lockOperation.LockedValue as IFieldReferenceOperation )?.Field;
        if (referencedSymbol is null)
        {
            return;
        }

        if (referencedSymbol.IsReadOnly || referencedSymbol.IsConst)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, lockOperation.LockedValue.Syntax.GetLocation(), referencedSymbol.Name));
    }
}