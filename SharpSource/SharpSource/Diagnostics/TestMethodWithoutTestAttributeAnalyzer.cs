using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodWithoutTestAttributeAnalyzer : DiagnosticAnalyzer
{
    private static DiagnosticDescriptor Rule => new(
        DiagnosticId.TestMethodWithoutTestAttribute,
        "A method might be missing a test attribute.",
        "Method {0} might be missing a test attribute",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS021-TestMethodWithoutTestAttribute.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var testClassAttributeSymbols = ImmutableArray.Create(
                compilationContext.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("NUnit.Framework.TestFixtureAttribute")
            );

            var testMethodAttributeSymbols = ImmutableArray.Create(
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.FactAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.TheoryAttribute")
            );

            var taskTypes = ImmutableArray.Create(
                compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task"),
                compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1"),
                compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask"),
                compilationContext.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1")
            );

            var allowedAdditionalAttributes = ImmutableArray.Create(
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.ClassDataAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("Xunit.InlineDataAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseAttribute"),
                compilationContext.Compilation.GetTypeByMetadataName("NUnit.Framework.TestCaseSourceAttribute")
            );

            compilationContext.RegisterSymbolAction(context => Analyze(context, testClassAttributeSymbols, testMethodAttributeSymbols, taskTypes, allowedAdditionalAttributes), SymbolKind.Method);
        });
    }

    private static void Analyze(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol?> testClassAttributeSymbols, ImmutableArray<INamedTypeSymbol?> testMethodAttributeSymbols, ImmutableArray<INamedTypeSymbol?> taskTypes, ImmutableArray<INamedTypeSymbol?> allowedAdditionalAttributes)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        // Don't trigger this for IDisposable implementations or lifetime hooks
        if (method.IsInterfaceImplementation() || method.IsOverride || method.MethodKind == MethodKind.Constructor)
        {
            return;
        }

        // Check if we're in a unit-test context
        // For NUnit and MSTest we can see if the enclosing class has a [TestClass] or [TestFixture] attribute
        // For xUnit.NET we will have to see if there are other methods in the current class that contain a [Fact] attribute
        if (method.ContainingType.TypeKind is not TypeKind.Class)
        {
            return;
        }

        // If it has different attributes then we won't bother with it either
        foreach (var attribute in method.GetAttributes())
        {
            if (allowedAdditionalAttributes.Any(a => SymbolEqualityComparer.Default.Equals(a, attribute.AttributeClass)))
            {
                continue;
            }

            return;
        }

        if (method.ReturnType is not { SpecialType: SpecialType.System_Void } && !taskTypes.Any(taskType => method.ReturnType.OriginalDefinition.Equals(taskType, SymbolEqualityComparer.Default)))
        {
            return;
        }

        var isTestClass = method.ContainingType.GetAttributes().Any(a => testClassAttributeSymbols.Any(testClassAttribute => a.AttributeClass?.Equals(testClassAttribute, SymbolEqualityComparer.Default) == true));
        if (!isTestClass)
        {
            // Look at other methods in the class to see if they have a test attribute
            // We do this only for xUnit.NET because the others should already have been caught with the previous test
            // If they weren't, it means the entire class wasn't marked as a test which is not in the scope of this analyzer
            foreach (var member in method.ContainingType.GetMembers().OfType<IMethodSymbol>())
            {
                var attributes = member.GetAttributes();
                var hasAnotherTestMethodAttribute = attributes.Any(a => testMethodAttributeSymbols.Any(tma => SymbolEqualityComparer.Default.Equals(tma, a.AttributeClass)));
                var hasAnotherTestRelatedAttribute = attributes.Any(a => allowedAdditionalAttributes.Any(aaa => SymbolEqualityComparer.Default.Equals(aaa, a.AttributeClass)));
                if (hasAnotherTestMethodAttribute || hasAnotherTestRelatedAttribute)
                {
                    isTestClass = true;
                    break;
                }
            }
        }

        if (!isTestClass)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
    }
}