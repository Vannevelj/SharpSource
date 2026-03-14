using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RedisResponseNotHandledAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.RedisResponseNotHandled,
        "A Redis response was not checked for errors",
        "The response of {0} was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS067-RedisResponseNotHandled.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    private static readonly string[] ResponseBaseTypeNames =
    [
        "Elastic.Transport.TransportResponse",
        "Elasticsearch.Net.IElasticsearchResponse",
        "OpenSearch.Net.IOpenSearchResponse"
    ];

    /// <summary>
    /// Members on the response (or its ApiCallDetails/ApiCall) that indicate error checking.
    /// </summary>
    private static readonly HashSet<string> ErrorCheckingMembers = new(
    [
        // Elastic v9
        "IsValidResponse",
        "ElasticsearchServerError",
        "TryGetOriginalException",

        // NEST v7 / OpenSearch
        "IsValid",
        "ServerError",
        "OriginalException",

        // Shared
        "DebugInformation",
        "ApiCallDetails",
        "ApiCall"
    ]);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var responseBaseTypes = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
            foreach (var typeName in ResponseBaseTypeNames)
            {
                var type = compilationContext.Compilation.GetTypeByMetadataName(typeName);
                if (type is not null)
                {
                    responseBaseTypes.Add(type);
                }
            }

            if (responseBaseTypes.Count == 0)
            {
                return;
            }

            compilationContext.RegisterOperationAction(
                c => Analyze(c, responseBaseTypes.ToImmutable()),
                OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, ImmutableArray<INamedTypeSymbol> responseBaseTypes)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var returnType = invocation.TargetMethod.ReturnType;

        // Unwrap Task<T> / ValueTask<T> to get the underlying response type
        if (returnType.IsGenericTaskType(out var wrappedType) && wrappedType is not null)
        {
            returnType = wrappedType;
        }

        if (!IsResponseType(returnType, responseBaseTypes))
        {
            return;
        }

        // Navigate past await and conversion operations to find the effective parent
        var parent = invocation.Parent;

        if (parent is IAwaitOperation)
        {
            parent = parent.Parent;
        }

        if (parent is IConversionOperation)
        {
            parent = parent.Parent;
        }

        // Case 1: Result is completely discarded (expression statement)
        if (parent is IExpressionStatementOperation)
        {
            var methodName = invocation.TargetMethod.Name;
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), methodName));
            return;
        }

        // Case 2: Result is assigned inside a try block but never checked for errors
        var localSymbol = GetAssignedLocal(parent);
        if (localSymbol is null)
        {
            return;
        }

        var tryOperation = FindEnclosingTry(invocation);
        if (tryOperation is null)
        {
            return;
        }

        if (!HasErrorCheckingAccess(tryOperation.Body, localSymbol))
        {
            var methodName = invocation.TargetMethod.Name;
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), methodName));
        }
    }

    private static ILocalSymbol? GetAssignedLocal(IOperation? parent)
    {
        // var response = ...
        if (parent is IVariableInitializerOperation { Parent: IVariableDeclaratorOperation declarator })
        {
            return declarator.Symbol;
        }

        // response = ...
        if (parent is ISimpleAssignmentOperation assignment && assignment.Target is ILocalReferenceOperation localRef)
        {
            return localRef.Local;
        }

        return null;
    }

    private static ITryOperation? FindEnclosingTry(IOperation operation)
    {
        var current = operation.Parent;
        while (current is not null)
        {
            if (current is ITryOperation tryOp)
            {
                return tryOp;
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool HasErrorCheckingAccess(IOperation scope, ILocalSymbol localSymbol)
    {
        foreach (var operation in scope.DescendantsAndSelf())
        {
            // response.IsValidResponse, response.ServerError, etc.
            if (operation is IMemberReferenceOperation memberRef &&
                memberRef.Instance is ILocalReferenceOperation localRef &&
                SymbolEqualityComparer.Default.Equals(localRef.Local, localSymbol) &&
                ErrorCheckingMembers.Contains(memberRef.Member.Name))
            {
                return true;
            }

            // response.TryGetOriginalException(out var ex)
            if (operation is IInvocationOperation methodInvocation &&
                methodInvocation.Instance is ILocalReferenceOperation methodLocalRef &&
                SymbolEqualityComparer.Default.Equals(methodLocalRef.Local, localSymbol) &&
                ErrorCheckingMembers.Contains(methodInvocation.TargetMethod.Name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsResponseType(ITypeSymbol type, ImmutableArray<INamedTypeSymbol> responseBaseTypes)
    {
        foreach (var responseBaseType in responseBaseTypes)
        {
            if (type.InheritsFrom(responseBaseType))
            {
                return true;
            }
        }

        return false;
    }
}
