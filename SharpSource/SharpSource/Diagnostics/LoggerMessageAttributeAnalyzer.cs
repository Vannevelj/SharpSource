using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LoggerMessageAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] LogMethodNames = { "Log", "LogTrace", "LogDebug", "LogInformation", "LogWarning", "LogError", "LogCritical" };

    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.LoggerMessageAttribute,
        "Use the LoggerMessage attribute for high-performance logging",
        "Use the LoggerMessage attribute for high-performance logging instead of {0}",
        Categories.Performance,
        DiagnosticSeverity.Info,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS065-LoggerMessageAttribute.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var loggerMessageAttributeSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerMessageAttribute");
            if (loggerMessageAttributeSymbol is null)
            {
                // LoggerMessageAttribute is not available in this compilation
                return;
            }

            var iloggerSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
            if (iloggerSymbol is null)
            {
                return;
            }

            var loggerExtensionsSymbol = compilationContext.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.LoggerExtensions");

            compilationContext.RegisterOperationAction(
                context => Analyze(context, (IInvocationOperation)context.Operation, iloggerSymbol, loggerExtensionsSymbol),
                OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, IInvocationOperation invocation, INamedTypeSymbol iloggerSymbol, INamedTypeSymbol? loggerExtensionsSymbol)
    {
        var targetMethod = invocation.TargetMethod;
        var methodName = targetMethod.Name;

        // Check if it's a log method we care about
        if (!LogMethodNames.Contains(methodName))
        {
            return;
        }

        // Check if it's on ILogger or LoggerExtensions
        var containingType = targetMethod.ContainingType;
        var isOnILogger = containingType.Equals(iloggerSymbol, SymbolEqualityComparer.Default) ||
                          containingType.AllInterfaces.Any(i => i.Equals(iloggerSymbol, SymbolEqualityComparer.Default));
        var isOnLoggerExtensions = loggerExtensionsSymbol is not null &&
                                   containingType.Equals(loggerExtensionsSymbol, SymbolEqualityComparer.Default);

        if (!isOnILogger && !isOnLoggerExtensions)
        {
            return;
        }

        // Don't flag if we're in top-level statements (no containing type to add the partial method to)
        // Top-level statements have a synthesized Program class, but no TypeDeclarationSyntax in the tree
        var hasContainingTypeDeclaration = invocation.Syntax.Ancestors()
            .Any(a => a is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax);
        if (!hasContainingTypeDeclaration)
        {
            return;
        }

        // Extract info needed for the code fix
        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("methodName", methodName);

        // Try to extract the log level
        string? logLevel = null;
        if (methodName != "Log")
        {
            if (methodName.Length >= 3)
            {
                // LogInformation -> Information, LogError -> Error, etc.
                logLevel = methodName.Substring(3);
            }
        }
        else
        {
            // For generic Log method, try to get the LogLevel from the first argument
            var logLevelArg = invocation.Arguments.FirstOrDefault(a =>
                a.Parameter?.Type?.Name == "LogLevel");
            if (logLevelArg?.Value is IFieldReferenceOperation fieldRef)
            {
                logLevel = fieldRef.Field.Name;
            }
        }

        properties.Add("logLevel", logLevel);

        // Try to find the message template
        var messageArg = invocation.Arguments.FirstOrDefault(a =>
            a.Parameter?.Name == "message" && a.Parameter.Type?.SpecialType == SpecialType.System_String);
        if (messageArg?.Value.ConstantValue.HasValue == true)
        {
            properties.Add("message", messageArg.Value.ConstantValue.Value?.ToString());
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), properties.ToImmutable(), methodName));
    }
}