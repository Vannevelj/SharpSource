using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;

        if (!method.Modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return;
        }

        // Don't trigger this for IDisposable implementations
        if (method is { Identifier.ValueText: "Dispose", ParameterList.Parameters.Count: 0 })
        {
            return;
        }

        // Check if we're in a unit-test context
        // For NUnit and MSTest we can see if the enclosing class/struct has a [TestClass] or [TestFixture] attribute
        // For xUnit.NET we will have to see if there are other methods in the current class that contain a [Fact] attribute

        var enclosingType = method.GetEnclosingTypeNode();
        if (!enclosingType.IsKind(SyntaxKind.ClassDeclaration) && !enclosingType.IsKind(SyntaxKind.StructDeclaration))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(enclosingType) is not INamedTypeSymbol symbol)
        {
            return;
        }

        var isTestClass = false;
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute?.AttributeClass == default)
            {
                continue;
            }

            if (attribute.AttributeClass.Name == "TestClass" ||
                attribute.AttributeClass.Name == "TestClassAttribute" ||
                attribute.AttributeClass.Name == "TestFixture" ||
                attribute.AttributeClass.Name == "TestFixtureAttribute")
            {
                isTestClass = true;
                break;
            }
        }

        // If it has different attributes then we won't bother with it either
        if (method.AttributeLists.SelectMany(x => x.Attributes).Any())
        {
            return;
        }

        if (!isTestClass)
        {
            // Look at other methods in the class to see if they have a test attribute
            // We do this only for xUnit.NET because the others should already have been caught with the previous test
            // If they weren't, it means the entire class wasn't marked as a test which is not in the scope of this analyzer

            foreach (var member in enclosingType.DescendantNodes().OfType<MethodDeclarationSyntax>(SyntaxKind.MethodDeclaration))
            {
                foreach (var attributeList in member.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (attribute.Name.ToString() == "Fact" || attribute.Name.ToString() == "Theory")
                        {
                            isTestClass = true;
                        }
                    }
                }
            }
        }

        if (!isTestClass)
        {
            return;
        }

        var returnType = context.SemanticModel.GetTypeInfo(method.ReturnType).Type;
        var voidType = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Void);
        var taskType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");
        var taskTType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
        if (returnType != default && !(
            returnType.Equals(voidType, SymbolEqualityComparer.Default) ||
            returnType.Equals(taskType, SymbolEqualityComparer.Default) ||
            returnType.OriginalDefinition.Equals(taskTType, SymbolEqualityComparer.Default) ))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), method.Identifier));
    }
}