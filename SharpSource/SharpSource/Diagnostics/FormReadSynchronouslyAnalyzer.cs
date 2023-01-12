using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FormReadSynchronouslyAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.FormReadSynchronously,
        "Synchronously accessed HttpRequest.Form which uses sync-over-async. Use HttpRequest.ReadFormAsync() instead",
        "Synchronously accessed HttpRequest.Form. Use HttpRequest.ReadFormAsync() instead",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS056-FormReadSynchronously.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var httpRequestSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Http.HttpRequest");
            var formSymbol = httpRequestSymbol?.GetMembers("Form").OfType<IPropertySymbol>().FirstOrDefault();
            if (formSymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => Analyze(context, formSymbol), OperationKind.PropertyReference);
            }
        });
    }

    private static void Analyze(OperationAnalysisContext context, IPropertySymbol formSymbol)
    {
        var propertyReference = (IPropertyReferenceOperation)context.Operation;
        if (propertyReference.Parent is INameOfOperation)
        {
            return;
        }

        if (formSymbol.Equals(propertyReference.Property, SymbolEqualityComparer.Default))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, propertyReference.Syntax.GetLocation()));
        }
    }
}