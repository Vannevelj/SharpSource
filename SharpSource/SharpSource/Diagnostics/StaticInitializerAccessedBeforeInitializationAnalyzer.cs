using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticInitializerAccessedBeforeInitializationAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "A static field relies on the value of another static field which is defined in the same type. Static fields are initialized in order of appearance.";

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StaticInitializerAccessedBeforeInitialization,
        Title,
        "{0} accesses {1} but both are marked as static and {1} will not be initialized when it is used",
        Categories.Correctness,
        DiagnosticSeverity.Error,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS045-StaticInitializerAccessedBeforeInitialization.md"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterSymbolStartAction(context =>
        {
            var symbol = (INamedTypeSymbol)context.Symbol;

            // Collect potential symbols that shouldn't be referenced before initialized.
            var disallowedSymbolsInOrder = symbol.GetMembers()
                                                 .Where(m => m.IsStatic && m is not IFieldSymbol { IsConst: true } && m.Kind is SymbolKind.Field or SymbolKind.Property)
                                                 .Select((s, index) => new { symbol = s, index })
                                                 .ToDictionary(x => x.symbol, x => x.index, SymbolEqualityComparer.Default);
            context.RegisterOperationBlockStartAction(context =>
            {
                var owningSymbol = context.OwningSymbol;
                if (!owningSymbol.IsStatic || !disallowedSymbolsInOrder.TryGetValue(owningSymbol, out var owningIndex))
                {
                    return;
                }

                context.RegisterOperationAction(context =>
                {
                    ISymbol? referencedSymbol = context.Operation switch
                    {
                        IFieldReferenceOperation fieldReference => fieldReference.Field,
                        IPropertyReferenceOperation propertyReference => propertyReference.Property,
                        _ => null, // never happens.
                    };

                    if (referencedSymbol is not { IsStatic: true })
                    {
                        return;
                    }

                    var operation = context.Operation.Parent;
                    while (operation is not null)
                    {
                        if (operation.Kind is OperationKind.AnonymousFunction or OperationKind.NameOf or OperationKind.Invocation)
                        {
                            return;
                        }

                        operation = operation.Parent;
                    }

                    if (disallowedSymbolsInOrder.TryGetValue(referencedSymbol, out var referencedIndex) && owningIndex < referencedIndex)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), owningSymbol.Name, referencedSymbol.Name));
                    }
                }, OperationKind.FieldReference, OperationKind.PropertyReference);
            });
        }, SymbolKind.NamedType);
    }
}