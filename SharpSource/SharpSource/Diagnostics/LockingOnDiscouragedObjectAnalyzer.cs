using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LockingOnDiscouragedObjectAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Title = "A lock was taken using an instance of a discouraged type. System.String, System.Type and 'this' references can all lead to deadlocks and should be replaced with a System.Object instance instead.";

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LockingOnDiscouragedObject,
        Title,
        "A lock was used on an object of type {0} which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor RuleThis => new(
        DiagnosticId.LockingOnDiscouragedObject,
        Title,
        "A lock was used referencing 'this' which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, RuleThis);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.LockStatement);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var lockStatement = (LockStatementSyntax)context.Node;

        if (lockStatement.Expression is ThisExpressionSyntax)
        {
            context.ReportDiagnostic(Diagnostic.Create(RuleThis, lockStatement.Expression.GetLocation()));
            return;
        }

        var symbol = context.SemanticModel.GetTypeInfo(lockStatement.Expression).Type;
        if (symbol?.SpecialType is SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, lockStatement.Expression.GetLocation(), "string"));
            return;
        }

        if (symbol?.IsDefinedInSystemAssembly() == true && symbol.Name == "Type")
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, lockStatement.Expression.GetLocation(), "Type"));
            return;
        }
    }
}