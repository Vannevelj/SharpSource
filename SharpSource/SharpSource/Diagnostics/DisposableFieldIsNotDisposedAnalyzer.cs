using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableFieldIsNotDisposedAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.DisposableFieldIsNotDisposed,
        "A disposable field is not disposed by the containing type",
        "Disposable field {0} in type {1} is not disposed",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS066-DisposableFieldIsNotDisposed.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var disposableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.IDisposable");
            if (disposableSymbol is null)
            {
                return;
            }

            var asyncDisposableSymbol = compilationContext.Compilation.GetTypeByMetadataName("System.IAsyncDisposable");
            var analyzedTypes = new ConcurrentBag<INamedTypeSymbol>();
            var methodData = new ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult>(SymbolEqualityComparer.Default);

            // We collect per-symbol operation data during symbol analysis because this is where Roslyn gives us
            // access to method bodies in an analyzer-friendly way.
            compilationContext.RegisterSymbolStartAction(c => AnalyzeTypeStart(c, analyzedTypes, methodData), SymbolKind.NamedType);

            // We report at compilation end because resolving indirect disposal paths may require data from several
            // symbols at once: methods on the current type, local functions, properties and even default interface
            // implementations. Waiting until the end ensures the cross-symbol graph is complete before we walk it.
            compilationContext.RegisterCompilationEndAction(c => AnalyzeCompilationEnd(c, disposableSymbol, asyncDisposableSymbol, analyzedTypes, methodData));
        });
    }

    private static void AnalyzeTypeStart(
        SymbolStartAnalysisContext context,
        ConcurrentBag<INamedTypeSymbol> analyzedTypes,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not ( TypeKind.Class or TypeKind.Struct or TypeKind.Interface ))
        {
            return;
        }

        if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
        {
            analyzedTypes.Add(type);
        }

        // Record method/property usage for this symbol as Roslyn visits it. The final diagnostic decision is delayed
        // until compilation end, once all reachable helpers and interface members have had a chance to contribute data.
        context.RegisterOperationBlockAction(c => AnalyzeOperationBlock(c, methodData, type));
    }

    private static void AnalyzeOperationBlock(
        OperationBlockAnalysisContext context,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData,
        INamedTypeSymbol containingType)
    {
        if (context.OwningSymbol is not IMethodSymbol method)
        {
            return;
        }

        var referencedFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var referencedProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        var invokedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        foreach (var operationBlock in context.OperationBlocks)
        {
            VisitOperation(operationBlock, referencedFields, referencedProperties, invokedMethods, containingType);
            RegisterLocalFunctions(operationBlock, methodData, containingType);
        }

        methodData[method.OriginalDefinition] = new MethodAnalysisResult(referencedFields, referencedProperties, invokedMethods);
    }

    private static void AnalyzeCompilationEnd(
        CompilationAnalysisContext context,
        INamedTypeSymbol disposableSymbol,
        INamedTypeSymbol? asyncDisposableSymbol,
        ConcurrentBag<INamedTypeSymbol> analyzedTypes,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        // Multiple symbol-start callbacks may add the same type in partial or repeated analysis scenarios.
        // Deduplicate first, then evaluate each concrete type against the complete method/property graph.
        var uniqueTypes = new HashSet<INamedTypeSymbol>(analyzedTypes, SymbolEqualityComparer.Default);

        foreach (var type in uniqueTypes)
        {
            AnalyzeType(context, type, disposableSymbol, asyncDisposableSymbol, methodData);
        }
    }

    private static void AnalyzeType(
        CompilationAnalysisContext context,
        INamedTypeSymbol type,
        INamedTypeSymbol disposableSymbol,
        INamedTypeSymbol? asyncDisposableSymbol,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        // First identify the instance fields this type actually owns and is responsible for cleaning up.
        var disposableFields = type
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(IsTrackableField)
            .Where(field => ImplementsDisposableContract(field.Type, disposableSymbol, asyncDisposableSymbol))
            .ToImmutableArray();

        if (disposableFields.IsDefaultOrEmpty)
        {
            return;
        }

        var candidateFields = new HashSet<IFieldSymbol>(disposableFields, SymbolEqualityComparer.Default);

        // Auto-properties and similar compiler-generated members can surface as backing fields. We map those back
        // to their property symbol so property-based disposal paths can still mark the underlying storage as handled.
        var propertyBackings = disposableFields
            .Where(field => field.AssociatedSymbol is IPropertySymbol)
            .ToDictionary(field => ( (IPropertySymbol)field.AssociatedSymbol! ).OriginalDefinition, field => field, SymbolEqualityComparer.Default);

        // Entry methods are the roots of the disposal graph: direct Dispose members, finalizers, and any applicable
        // default interface implementations.
        var entryMethods = GetEntryMethods(type, disposableSymbol, asyncDisposableSymbol, methodData);
        var implementsDisposalContract = type.AllInterfaces.Any(@interface =>
            SymbolEqualityComparer.Default.Equals(@interface, disposableSymbol) ||
            ( asyncDisposableSymbol is not null && SymbolEqualityComparer.Default.Equals(@interface, asyncDisposableSymbol) ));

        if (!implementsDisposalContract && entryMethods.IsDefaultOrEmpty)
        {
            return;
        }

        if (entryMethods.IsDefaultOrEmpty)
        {
            if (type.IsAbstract || !implementsDisposalContract)
            {
                return;
            }

            // The type participates in disposal but provides no concrete entry point we can follow, so every owned
            // disposable field is considered unhandled.
            foreach (var field in disposableFields)
            {
                ReportDiagnostic(context, field, type);
            }

            return;
        }

        var handledFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
        var visitedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var methodsToVisit = new Queue<IMethodSymbol>(entryMethods);

        // Walk the disposal graph breadth-first. Each visited method contributes directly referenced fields,
        // property-based indirections, and further helper methods to inspect.
        while (methodsToVisit.Count > 0)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var method = methodsToVisit.Dequeue().OriginalDefinition;
            if (!visitedMethods.Add(method))
            {
                continue;
            }

            if (!methodData.TryGetValue(method, out var analysis))
            {
                continue;
            }

            foreach (var field in analysis.ReferencedFields)
            {
                if (candidateFields.Contains(field))
                {
                    handledFields.Add(field);
                }
            }

            foreach (var property in analysis.ReferencedProperties)
            {
                var resolvedProperty = ResolvePropertyForType(property, type, methodData);
                if (resolvedProperty is not null && propertyBackings.TryGetValue(resolvedProperty.OriginalDefinition, out var backingField))
                {
                    handledFields.Add(backingField);
                }
            }

            foreach (var invokedMethod in analysis.InvokedMethods)
            {
                var resolvedMethod = ResolveMethodForType(invokedMethod, type, methodData);
                if (resolvedMethod is not null)
                {
                    methodsToVisit.Enqueue(resolvedMethod.OriginalDefinition);
                }
            }
        }

        // Anything never reached from the disposal graph is reported as undisposed.
        foreach (var field in disposableFields)
        {
            if (!handledFields.Contains(field))
            {
                ReportDiagnostic(context, field, type);
            }
        }
    }

    /// <summary>
    /// Collects the disposal entry points for a concrete type.
    /// </summary>
    /// <remarks>
    /// This starts with disposal members declared directly on the type itself, such as <c>Dispose()</c>,
    /// <c>DisposeAsync()</c>, overloads like <c>Dispose(bool)</c>, and finalizers.
    /// <para>
    /// It then also inspects implemented interfaces for disposal members, because a type may rely on a default
    /// interface implementation instead of providing its own concrete method body. In that case we resolve the
    /// interface member back to the body that will actually execute for this type and use that as a graph entry point.
    /// </para>
    /// <para>
    /// The returned methods are the roots for the later traversal that follows helper-method calls, property accessors,
    /// and interface indirection to determine whether each owned disposable field is eventually handled.
    /// </para>
    /// </remarks>
    private static ImmutableArray<IMethodSymbol> GetEntryMethods(
        INamedTypeSymbol type,
        INamedTypeSymbol disposableSymbol,
        INamedTypeSymbol? asyncDisposableSymbol,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        var entryMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (!method.IsImplicitlyDeclared && !method.IsStatic && !method.IsAbstract && IsDisposalEntryPoint(method, disposableSymbol, asyncDisposableSymbol))
            {
                entryMethods.Add(method.OriginalDefinition);
            }
        }

        foreach (var @interface in type.AllInterfaces)
        {
            foreach (var interfaceMethod in @interface.GetMembers().OfType<IMethodSymbol>())
            {
                if (!IsDisposalEntryPoint(interfaceMethod, disposableSymbol, asyncDisposableSymbol))
                {
                    continue;
                }

                var resolvedMethod = ResolveMethodForType(interfaceMethod, type, methodData);
                if (resolvedMethod is not null)
                {
                    entryMethods.Add(resolvedMethod.OriginalDefinition);
                }
            }
        }

        return entryMethods.ToImmutableArray();
    }

    private static bool ImplementsDisposableContract(ITypeSymbol type, INamedTypeSymbol disposableSymbol, INamedTypeSymbol? asyncDisposableSymbol)
        => type.InheritsFrom(disposableSymbol) || ( asyncDisposableSymbol is not null && type.InheritsFrom(asyncDisposableSymbol) );

    private static bool IsTrackableField(IFieldSymbol field)
        => !field.IsStatic && !field.IsConst && ( !field.IsImplicitlyDeclared || field.AssociatedSymbol is IPropertySymbol );

    private static bool IsDisposalEntryPoint(IMethodSymbol method, INamedTypeSymbol disposableSymbol, INamedTypeSymbol? asyncDisposableSymbol)
    {
        if (method.MethodKind is MethodKind.Destructor)
        {
            return true;
        }

        if (method.Name.StartsWith("Dispose", System.StringComparison.Ordinal))
        {
            return true;
        }

        foreach (var implementation in method.ExplicitInterfaceImplementations)
        {
            if (implementation.Name == "Dispose" && SymbolEqualityComparer.Default.Equals(implementation.ContainingType, disposableSymbol))
            {
                return true;
            }

            if (implementation.Name == "DisposeAsync" && asyncDisposableSymbol is not null && SymbolEqualityComparer.Default.Equals(implementation.ContainingType, asyncDisposableSymbol))
            {
                return true;
            }
        }

        return false;
    }

    private static void VisitOperation(
        IOperation operation,
        HashSet<IFieldSymbol> referencedFields,
        HashSet<IPropertySymbol> referencedProperties,
        HashSet<IMethodSymbol> invokedMethods,
        INamedTypeSymbol containingType)
    {
        if (operation is ILocalFunctionOperation)
        {
            return;
        }

        if (operation is IFieldReferenceOperation fieldReference && IsTrackableField(fieldReference.Field))
        {
            referencedFields.Add(fieldReference.Field.OriginalDefinition);
        }

        if (operation is IPropertyReferenceOperation propertyReference && !propertyReference.Property.IsStatic)
        {
            referencedProperties.Add(propertyReference.Property.OriginalDefinition);

            foreach (var accessor in GetAccessors(propertyReference))
            {
                if (IsReachableHelperMethod(accessor, containingType))
                {
                    invokedMethods.Add(accessor.OriginalDefinition);
                }
            }
        }

        if (operation is IInvocationOperation invocation && IsReachableHelperMethod(invocation.TargetMethod, containingType))
        {
            invokedMethods.Add(invocation.TargetMethod.OriginalDefinition);
        }

        foreach (var child in operation.ChildOperations)
        {
            VisitOperation(child, referencedFields, referencedProperties, invokedMethods, containingType);
        }
    }

    private static IEnumerable<IMethodSymbol> GetAccessors(IPropertyReferenceOperation propertyReference)
    {
        if (propertyReference.Parent is ISimpleAssignmentOperation assignment && assignment.Target == propertyReference)
        {
            if (propertyReference.Property.SetMethod is not null)
            {
                yield return propertyReference.Property.SetMethod;
            }

            yield break;
        }

        if (propertyReference.Property.GetMethod is not null)
        {
            yield return propertyReference.Property.GetMethod;
        }
    }

    private static bool IsReachableHelperMethod(IMethodSymbol method, INamedTypeSymbol containingType)
        => method.MethodKind == MethodKind.LocalFunction ||
           ( !method.IsStatic && (
               SymbolEqualityComparer.Default.Equals(method.ContainingType, containingType) ||
               method.ContainingType.TypeKind == TypeKind.Interface ) );

    private static void RegisterLocalFunctions(
        IOperation operation,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData,
        INamedTypeSymbol containingType)
    {
        foreach (var child in operation.ChildOperations)
        {
            if (child is ILocalFunctionOperation localFunction)
            {
                var referencedFields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);
                var referencedProperties = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
                var invokedMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

                foreach (var localChild in localFunction.ChildOperations)
                {
                    VisitOperation(localChild, referencedFields, referencedProperties, invokedMethods, containingType);
                    RegisterLocalFunctions(localChild, methodData, containingType);
                }

                methodData[localFunction.Symbol.OriginalDefinition] = new MethodAnalysisResult(referencedFields, referencedProperties, invokedMethods);
                continue;
            }

            RegisterLocalFunctions(child, methodData, containingType);
        }
    }

    private static IMethodSymbol? ResolveMethodForType(
        IMethodSymbol method,
        INamedTypeSymbol type,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        method = method.OriginalDefinition;

        if (method.MethodKind == MethodKind.LocalFunction)
        {
            return method;
        }

        if (SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
        {
            return method;
        }

        if (method.ContainingType.TypeKind != TypeKind.Interface)
        {
            return null;
        }

        if (method.AssociatedSymbol is IPropertySymbol property)
        {
            var resolvedProperty = ResolvePropertyForType(property, type, methodData);
            if (resolvedProperty is null)
            {
                return null;
            }

            return method.MethodKind switch
            {
                MethodKind.PropertyGet => resolvedProperty.GetMethod,
                MethodKind.PropertySet => resolvedProperty.SetMethod,
                _ => null,
            };
        }

        var implementation = type.FindImplementationForInterfaceMember(method);
        if (implementation is IMethodSymbol implementedMethod)
        {
            if (implementedMethod.ContainingType.TypeKind == TypeKind.Interface && methodData.ContainsKey(method))
            {
                return method;
            }

            return implementedMethod;
        }

        if (methodData.ContainsKey(method))
        {
            return method;
        }

        return null;
    }

    private static IPropertySymbol? ResolvePropertyForType(
        IPropertySymbol property,
        INamedTypeSymbol type,
        ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
    {
        property = property.OriginalDefinition;

        if (SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
        {
            return property;
        }

        if (property.ContainingType.TypeKind != TypeKind.Interface)
        {
            return null;
        }

        var implementation = type.FindImplementationForInterfaceMember(property);
        if (implementation is IPropertySymbol implementedProperty)
        {
            if (implementedProperty.ContainingType.TypeKind == TypeKind.Interface && HasAccessorData(property, methodData))
            {
                return property;
            }

            return implementedProperty;
        }

        if (HasAccessorData(property, methodData))
        {
            return property;
        }

        return null;
    }

    private static bool HasAccessorData(IPropertySymbol property, ConcurrentDictionary<IMethodSymbol, MethodAnalysisResult> methodData)
        => ( property.GetMethod is not null && methodData.ContainsKey(property.GetMethod.OriginalDefinition) ) ||
           ( property.SetMethod is not null && methodData.ContainsKey(property.SetMethod.OriginalDefinition) );

    private static void ReportDiagnostic(CompilationAnalysisContext context, IFieldSymbol field, INamedTypeSymbol type)
    {
        var location = field.AssociatedSymbol?.Locations.FirstOrDefault() ?? field.Locations[0];
        var name = field.AssociatedSymbol?.Name ?? field.Name;

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, name, type.Name));
    }

    private sealed class MethodAnalysisResult
    {
        public MethodAnalysisResult(
            HashSet<IFieldSymbol> referencedFields,
            HashSet<IPropertySymbol> referencedProperties,
            HashSet<IMethodSymbol> invokedMethods)
        {
            ReferencedFields = referencedFields;
            ReferencedProperties = referencedProperties;
            InvokedMethods = invokedMethods;
        }

        public HashSet<IFieldSymbol> ReferencedFields { get; }

        public HashSet<IPropertySymbol> ReferencedProperties { get; }

        public HashSet<IMethodSymbol> InvokedMethods { get; }
    }
}