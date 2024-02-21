using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DateTimeNowAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.DateTimeNow,
        "DateTime.Now was referenced",
        "Use DateTime.UtcNow to get a locale-independent value",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS002-DateTimeNow.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var dateTimeSymbol = compilationContext.Compilation.GetSpecialType(SpecialType.System_DateTime);
            var nowPropertySymbol = dateTimeSymbol.GetMembers("Now").OfType<IPropertySymbol>().FirstOrDefault();
            if (nowPropertySymbol is not null)
            {
                compilationContext.RegisterOperationAction(context => AnalyzePropertyReference(context, nowPropertySymbol), OperationKind.PropertyReference);
            }
        });
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context, IPropertySymbol nowProperty)
    {
        var propertyReference = (IPropertyReferenceOperation)context.Operation;
        if (propertyReference.Parent is INameOfOperation)
        {
            return;
        }

        if (nowProperty.Equals(propertyReference.Property, SymbolEqualityComparer.Default))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, propertyReference.Syntax.GetLocation()));
        }
    }
}