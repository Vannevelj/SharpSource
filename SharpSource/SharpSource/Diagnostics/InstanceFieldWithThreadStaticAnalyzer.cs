using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InstanceFieldWithThreadStaticAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.InstanceFieldWithThreadStatic,
        "[ThreadStatic] can only be used on static fields",
        "Field {0} is marked as [ThreadStatic] but is not static",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS042-InstanceFieldWithThreadStatic.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var threadStaticSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.ThreadStaticAttribute");
            if (threadStaticSymbol is not null)
            {
                compilationContext.RegisterSymbolAction(context => Analyze(context, threadStaticSymbol), SymbolKind.Field);
            }
        });
    }

    private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol threadStaticSymbol)
    {
        var field = (IFieldSymbol)context.Symbol;
        if (field.IsStatic)
        {
            return;
        }

        if (!field.GetAttributes().Any(a => threadStaticSymbol.Equals(a.AttributeClass, SymbolEqualityComparer.Default)))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, field.Locations[0], field.Name));
    }
}