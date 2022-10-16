using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LockingOnMutableReferenceAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LockingOnMutableReference,
        "A lock was obtained on a mutable field which can lead to deadlocks when a new value is assigned. Mark the field as readonly to prevent re-assignment after a lock is taken.",
        "A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned.",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS051-LockingOnMutableReference.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.LockStatement);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        
    }
}