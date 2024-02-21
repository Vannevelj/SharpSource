using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NewtonsoftMixedWithSystemTextJsonAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.NewtonsoftMixedWithSystemTextJson,
        "An attempt is made to (de-)serialize an object which combines System.Text.Json and Newtonsoft.Json. Attributes from one won't be adhered to in the other and should not be mixed.",
        "Attempting to {0} an object annotated with {1} through {2}",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS054-NewtonsoftMixedWithSystemTextJson.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var newtonSoftSerializer = compilationContext.Compilation.GetTypeByMetadataName("Newtonsoft.Json.JsonConvert");
            var systemTextSerializer = compilationContext.Compilation.GetTypeByMetadataName("System.Text.Json.JsonSerializer");

            var newtonsoftAttribute = compilationContext.Compilation.GetTypeByMetadataName("Newtonsoft.Json.JsonPropertyAttribute");
            var systemTextAttribute = compilationContext.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonPropertyNameAttribute");

            // Only execute the analyzer if both types exist
            if (newtonSoftSerializer is not null && systemTextSerializer is not null && newtonsoftAttribute is not null && systemTextAttribute is not null)
            {
                var newtonsoftSerializers = newtonSoftSerializer.GetMembers("SerializeObject").OfType<IMethodSymbol>().ToArray();
                var newtonsoftDeserializers = newtonSoftSerializer.GetMembers("DeserializeObject").OfType<IMethodSymbol>().ToArray();

                var systemTextSerializers = systemTextSerializer.GetMembers("Serialize").OfType<IMethodSymbol>().ToArray();
                var systemTextDeserializers = systemTextSerializer.GetMembers("Deserialize").OfType<IMethodSymbol>().ToArray();

                compilationContext.RegisterOperationAction((context) => Analyze(context, newtonsoftAttribute, systemTextAttribute, newtonsoftSerializers, newtonsoftDeserializers, systemTextSerializers, systemTextDeserializers), OperationKind.Invocation);
            }
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol newtonsoftAttribute, INamedTypeSymbol systemTextAttribute, IMethodSymbol[] newtonsoftSerializers, IMethodSymbol[] newtonsoftDeserializers, IMethodSymbol[] systemTextSerializers, IMethodSymbol[] systemTextDeserializers)
    {
        var invocation = (IInvocationOperation)context.Operation;

        var invokedNewtonsoftSerializer = newtonsoftSerializers.FirstOrDefault(s => s.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default));
        if (invokedNewtonsoftSerializer is not null)
        {
            var argument = invocation.Arguments.FirstOrDefault();
            var passedArgument = argument?.SemanticModel?.GetTypeInfo(( (ArgumentSyntax)argument.Syntax ).Expression).Type;
            Check(invocation, passedArgument, systemTextAttribute, "serialize", context);
            return;
        }

        var invokedNewtonsoftDeserializer = newtonsoftDeserializers.FirstOrDefault(s => s.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default));
        if (invokedNewtonsoftDeserializer is not null)
        {
            var passedArgument = invocation.TargetMethod.TypeArguments.FirstOrDefault();
            Check(invocation, passedArgument, systemTextAttribute, "deserialize", context);
            return;
        }

        var invokedSystemTextSerializer = systemTextSerializers.FirstOrDefault(s => s.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default));
        if (invokedSystemTextSerializer is not null)
        {
            var passedArgument = invocation.TargetMethod.TypeArguments.FirstOrDefault();
            Check(invocation, passedArgument, newtonsoftAttribute, "serialize", context);
            return;
        }

        var invokedSystemTextDeserializer = systemTextDeserializers.FirstOrDefault(s => s.Equals(invocation.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default));
        if (invokedSystemTextDeserializer is not null)
        {
            var passedArgument = invocation.TargetMethod.TypeArguments.FirstOrDefault();
            Check(invocation, passedArgument, newtonsoftAttribute, "deserialize", context);
            return;
        }
    }

    private static void Check(IInvocationOperation invocation, ITypeSymbol? argument, INamedTypeSymbol opposingAttributeType, string operation, OperationAnalysisContext context)
    {
        if (argument is null)
        {
            return;
        }

        foreach (var member in argument.GetMembers())
        {
            var incompatibleAttribute = member.GetAttributes().FirstOrDefault(a => opposingAttributeType.Equals(a.AttributeClass, SymbolEqualityComparer.Default));
            if (incompatibleAttribute is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    invocation.Syntax.GetLocation(),
                    operation,
                    incompatibleAttribute.AttributeClass?.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                    invocation.TargetMethod.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
                ));
            }
        }
    }
}