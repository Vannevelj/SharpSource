using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CollectionManipulatedDuringTraversal : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.CollectionManipulatedDuringTraversal,
        "A collection was modified while it was being iterated over. Make a copy first or avoid iterations while the loop is in progress to avoid an InvalidOperationException exception at runtime",
        "Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS057-CollectionManipulatedDuringTraversal.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var listSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            var setSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            IEnumerable<ISymbol> getSymbols(string metadataName, params string[] methods)
            {
                return compilationContext.Compilation.GetTypeByMetadataName(metadataName)?.GetAllMembers(methods) ?? Enumerable.Empty<ISymbol>();
            }

            var allProhibitedMethods = Enumerable.Empty<ISymbol>()
                .Concat(getSymbols("System.Collections.Generic.List`1", "Add", "Remove", "Insert", "AddRange", "Clear", "InsertRange", "RemoveAll", "RemoveAt", "RemoveRange", "Reverse", "Sort"))
                .Concat(getSymbols("System.Collections.Generic.Dictionary`2", "Add", "Clear", "Remove", "TryAdd"))
                .Concat(getSymbols("System.Collections.Generic.HashSet`1", "Add", "Clear", "ExceptWith", "IntersectWith", "Remove", "RemoveWhere", "SymmetricExceptWith", "UnionWith"))
                .Concat(getSymbols("System.Collections.Generic.Stack`1", "Clear", "Pop", "Push", "TryPop"))
                .Concat(getSymbols("System.Collections.Generic.Queue`1", "Clear", "Dequeue", "Enqueue", "TryDequeue"))
                .Concat(getSymbols("System.Collections.Generic.SortedDictionary`2", "Add", "Clear", "Remove"))
                .Concat(getSymbols("System.Collections.Generic.SortedList`2", "Add", "Remove", "RemoveAt", "SetValueAtIndex"))
                .Concat(getSymbols("System.Collections.Generic.SortedSet`1", "Add", "Clear", "ExceptWith", "IntersectWith", "Remove", "RemoveWhere", "Reverse", "SymmetricExceptWith", "UnionWith"))
                .Concat(getSymbols("System.Collections.Generic.ICollection`1", "Add", "Clear", "Remove"))
                .Concat(getSymbols("System.Collections.Generic.IList`1", "Insert", "RemoveAt"))
                .Concat(getSymbols("System.Collections.Generic.ISet`1", "Add", "ExceptWith", "IntersectWith", "SymmetricExceptWith", "UnionWith"))
                .Concat(getSymbols("System.Collections.Generic.IDictionary`2", "Add", "Remove"))
                .OfType<IMethodSymbol>()
                .ToArray();

            if (allProhibitedMethods is { Length: > 0 })
            {
                compilationContext.RegisterOperationAction(context => Analyze(context, allProhibitedMethods), OperationKind.Loop);
            }
        });
    }

    private static void Analyze(OperationAnalysisContext context, IMethodSymbol[] prohibitedInvocations)
    {
        var loopOperation = (ILoopOperation)context.Operation;
        if (loopOperation.LoopKind is not ( LoopKind.For or LoopKind.ForEach ))
        {
            return;
        }

        var (collectionBeingIteratedOn, instanceOfIteration) = loopOperation switch
        {
            IForEachLoopOperation { Collection: IConversionOperation conv } => (GetReferencedSymbol(conv.Operand), GetReferencedInstance(conv.Operand)),
            IForLoopOperation { Condition: IBinaryOperation { RightOperand: IMemberReferenceOperation memberRef } } => (GetReferencedSymbol(memberRef.Instance), GetReferencedInstance(memberRef.Instance)),
            _ => default
        };

        if (collectionBeingIteratedOn == default || instanceOfIteration == default)
        {
            return;
        }

        foreach (var invocation in loopOperation.Body.Descendants().TakeWhile(d => d.Kind is not OperationKind.AnonymousFunction).OfType<IInvocationOperation>())
        {
            if (prohibitedInvocations.Any(i => invocation.TargetMethod.OriginalDefinition.Equals(i, SymbolEqualityComparer.Default)))
            {
                var invokedSymbol = GetReferencedSymbol(invocation.Instance);
                if (collectionBeingIteratedOn.Equals(invokedSymbol, SymbolEqualityComparer.Default) && instanceOfIteration.Equals(GetReferencedInstance(invocation.Instance), SymbolEqualityComparer.Default))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                }
            }
        }
    }

    private static ISymbol? GetReferencedSymbol(IOperation? operation) => operation switch
    {
        ILocalReferenceOperation localRef => localRef.Local,
        IParameterReferenceOperation paramRef => paramRef.Parameter,
        IFieldReferenceOperation fieldRef => fieldRef.Field,
        IMemberReferenceOperation memberRef => memberRef.Member,
        IInstanceReferenceOperation instanceRef => instanceRef.Type,
        _ => default
    };
    private static ISymbol? GetReferencedInstance(IOperation? operation) => operation switch
    {
        ILocalReferenceOperation localRef => localRef.Local,
        IParameterReferenceOperation paramRef => paramRef.Parameter,
        IFieldReferenceOperation fieldRef => GetReferencedSymbol(fieldRef.Instance),
        IMemberReferenceOperation memberRef => GetReferencedSymbol(memberRef.Instance),
        IInstanceReferenceOperation instanceRef => instanceRef.Type,
        _ => default
    };
}