using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LockingOnDiscouragedObjectAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Title = "A lock was taken using an instance of a discouraged type. System.String, System.Type and 'this' references can all lead to deadlocks and should be replaced with a System.Object instance instead.";

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LockingOnDiscouragedObject,
        Title,
        "A lock was used on an object of type {0} which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS048-LockingOnDiscouragedObject.md");

    public static DiagnosticDescriptor RuleThis => new(
        DiagnosticId.LockingOnDiscouragedObject,
        Title,
        "A lock was used referencing 'this' which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS048-LockingOnDiscouragedObject.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleThis);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(context =>
        {
            var typeSymbol = context.Compilation.GetTypeByMetadataName("System.Type");
            context.RegisterOperationAction(context => AnalyzeLockOperation(context, typeSymbol), OperationKind.Lock);
        });
    }

    private static void AnalyzeLockOperation(OperationAnalysisContext context, INamedTypeSymbol? typeSymbol)
    {
        var lockOperation = (ILockOperation)context.Operation;
        if (lockOperation.LockedValue.Kind == OperationKind.InstanceReference)
        {
            context.ReportDiagnostic(Diagnostic.Create(RuleThis, lockOperation.LockedValue.Syntax.GetLocation()));
            return;
        }

        var symbol = lockOperation.LockedValue.Type;
        if (symbol?.SpecialType is SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, lockOperation.LockedValue.Syntax.GetLocation(), "string"));
            return;
        }

        if (symbol?.Equals(typeSymbol, SymbolEqualityComparer.Default) == true)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, lockOperation.LockedValue.Syntax.GetLocation(), "Type"));
            return;
        }
    }
}