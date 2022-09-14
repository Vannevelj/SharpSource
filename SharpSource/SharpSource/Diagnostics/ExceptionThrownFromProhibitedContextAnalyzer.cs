using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExceptionThrownFromProhibitedContextAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> AllowedExceptions = new(new string[] { "NotImplementedException", "NotSupportedException" });
    private static DiagnosticDescriptor ImplicitOperatorRule
        => new(DiagnosticId.ExceptionThrownFromImplicitOperator,
            "An exception is thrown from an implicit operator.",
            "An exception is thrown from implicit operator {0} in type {1}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS022-ExceptionThrownFromImplicitOperator.md");

    private static DiagnosticDescriptor PropertyGetterRule
        => new(DiagnosticId.ExceptionThrownFromPropertyGetter,
            "An exception is thrown from a property getter.",
            "An exception is thrown from the getter of property {0}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS023-ExceptionThrownFromPropertyGetter.md");

    private static DiagnosticDescriptor StaticConstructorRule
        => new(DiagnosticId.ExceptionThrownFromStaticConstructor,
            "An exception is thrown from a static constructor.",
            "An exception is thrown from {0} its static constructor", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS024-ExceptionThrownFromStaticConstructor.md");

    private static DiagnosticDescriptor FinallyBlockRule
        => new(DiagnosticId.ExceptionThrownFromFinallyBlock,
            "An exception is thrown from a finally block.",
            "An exception is thrown from a finally block", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS025-ExceptionThrownFromFinallyBlock.md");

    private static DiagnosticDescriptor EqualityOperatorRule
        => new(DiagnosticId.ExceptionThrownFromEqualityOperator,
            "An exception is thrown from an equality operator.",
            "An exception is thrown from the {0} operator between {1} and {2} in type {3}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS026-ExceptionThrownFromEqualityOperator.md");

    private static DiagnosticDescriptor DisposeRule
        => new(DiagnosticId.ExceptionThrownFromDispose,
            "An exception is thrown from a Dispose method.",
            "An exception is thrown from the {0} method in type {1}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS027-ExceptionThrownFromDispose.md");

    private static DiagnosticDescriptor FinalizerRule
        => new(DiagnosticId.ExceptionThrownFromFinalizer,
            "An exception is thrown from a finalizer method.",
            "An exception is thrown from the finalizer method in type {0}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS028-ExceptionThrownFromFinalizer.md");

    private static DiagnosticDescriptor GetHashCodeRule
        => new(DiagnosticId.ExceptionThrownFromGetHashCode,
            "An exception is thrown from a GetHashCode() method.",
            "An exception is thrown from the GetHashCode() method in type {0}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS029-ExceptionThrownFromGetHashCode.md");

    private static DiagnosticDescriptor EqualsRule
        => new(DiagnosticId.ExceptionThrownFromEquals,
            "An exception is thrown from an Equals() method.",
            "An exception is thrown from the Equals({0}) method in type {1}", Categories.ApiDesign, DiagnosticSeverity.Warning, true,
            helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS030-ExceptionThrownFromEquals.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            ImplicitOperatorRule,
            PropertyGetterRule,
            StaticConstructorRule,
            FinallyBlockRule,
            EqualityOperatorRule,
            DisposeRule,
            FinalizerRule,
            GetHashCodeRule,
            EqualsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ThrowStatement);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // Since the current node is a throw statement there is no symbol to be found. 
        // Therefore we look at whatever member is holding the statement (constructor, method, property, etc) and see what encloses that
        var throwStatement = (ThrowStatementSyntax)context.Node;
        var containingType = context.SemanticModel.GetEnclosingSymbol(throwStatement.SpanStart)?.ContainingType;
        if (containingType == null)
        {
            return;
        }

        var warningLocation = throwStatement.GetLocation();
        if (!ShouldWarnForExpression(throwStatement, context))
        {
            return;
        }

        var ancestor = throwStatement.FirstAncestorOfType(
            SyntaxKind.MethodDeclaration,
            SyntaxKind.GetAccessorDeclaration,
            SyntaxKind.FinallyClause,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DestructorDeclaration
        );

        if (ancestor.IsKind(SyntaxKind.MethodDeclaration))
        {
            var method = (MethodDeclarationSyntax)ancestor;
            var methodName = method.Identifier.ValueText;

            if (methodName == "Dispose")
            {
                var numberOfParameters = method.ParameterList.Parameters.Count;
                context.ReportDiagnostic(Diagnostic.Create(DisposeRule, warningLocation, numberOfParameters == 0 ? "Dispose()" : "Dispose(bool)", containingType.Name));
                return;
            }

            if (methodName == "GetHashCode")
            {
                // Make sure we're dealing with the actual members defined in 'Object' in case they're hidden in a subclass
                var currentMethodSymbol = context.SemanticModel.GetDeclaredSymbol(method);

                var objectSymbol = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Object);
                var objectGetHashCodeSymbol = objectSymbol.GetMembers("GetHashCode").Single();

                currentMethodSymbol = currentMethodSymbol?.GetBaseDefinition();

                if (currentMethodSymbol?.Equals(objectGetHashCodeSymbol, SymbolEqualityComparer.Default) == true)
                {
                    context.ReportDiagnostic(Diagnostic.Create(GetHashCodeRule, warningLocation, containingType.Name));
                    return;
                }
            }

            // We don't verify we're dealing with the 'Object' overridden method of 'Equals' because that would exclude Equals(T) in IEquatable
            // Furthermore we can expect to have multiple overloads of 'Equals' on argument type to provide a better equality comparison experience
            // This is not the case for GetHashCode() where we only expect one implementation
            if (methodName == "Equals" && method.ParameterList.Parameters.Count == 1)
            {
                var equalsParameter = method.ParameterList?.Parameters[0]?.Type?.ToString();
                context.ReportDiagnostic(Diagnostic.Create(EqualsRule, warningLocation, equalsParameter, containingType.Name));
                return;
            }
        }
        else if (ancestor.IsKind(SyntaxKind.GetAccessorDeclaration))
        {
            var property = ancestor.Ancestors().OfType<PropertyDeclarationSyntax>(SyntaxKind.PropertyDeclaration).FirstOrDefault();
            if (property == null)
            {
                return;
            }
            context.ReportDiagnostic(Diagnostic.Create(PropertyGetterRule, warningLocation, property.Identifier.ValueText));
            return;
        }
        else if (ancestor.IsKind(SyntaxKind.FinallyClause))
        {
            context.ReportDiagnostic(Diagnostic.Create(FinallyBlockRule, warningLocation));
            return;
        }
        else if (ancestor.IsKind(SyntaxKind.OperatorDeclaration))
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)ancestor;
            if (operatorDeclaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken) || operatorDeclaration.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
            {
                var operatorToken = operatorDeclaration.OperatorToken.ValueText;
                var firstType = operatorDeclaration.ParameterList?.Parameters[0]?.Type?.ToString();
                var secondType = operatorDeclaration.ParameterList?.Parameters[1]?.Type?.ToString();
                context.ReportDiagnostic(Diagnostic.Create(EqualityOperatorRule, warningLocation, operatorToken, firstType, secondType, containingType.Name));
                return;
            }
        }
        else if (ancestor.IsKind(SyntaxKind.ConversionOperatorDeclaration))
        {
            var conversionOperatorDeclaration = (ConversionOperatorDeclarationSyntax)ancestor;
            if (conversionOperatorDeclaration.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(ImplicitOperatorRule, warningLocation, conversionOperatorDeclaration.Type.ToString(), containingType.Name));
                return;
            }
        }
        else if (ancestor.IsKind(SyntaxKind.ConstructorDeclaration))
        {
            var constructor = (ConstructorDeclarationSyntax)ancestor;
            if (constructor.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(StaticConstructorRule, warningLocation, containingType.Name));
                return;
            }
        }
        else if (ancestor.IsKind(SyntaxKind.DestructorDeclaration))
        {
            context.ReportDiagnostic(Diagnostic.Create(FinalizerRule, warningLocation, containingType.Name));
            return;
        }
    }

    private bool ShouldWarnForExpression(ThrowStatementSyntax statement, SyntaxNodeAnalysisContext context)
    {
        if (statement.Expression == default)
        {
            // Not enough information to decide the exact type being thrown, let's just always warn
            return true;
        }

        var typeBeingThrown = statement.Expression switch
        {
            ObjectCreationExpressionSyntax objectCreation => context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol,
            MemberAccessExpressionSyntax memberAccess => context.SemanticModel.GetSymbolInfo(memberAccess.Name).Symbol,
            _ => default
        };

        return typeBeingThrown is null || !AllowedExceptions.Contains(typeBeingThrown.Name);
    }
}