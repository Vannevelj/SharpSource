using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionThrownFromProhibitedContextAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.ExceptionsCategory;

        private static DiagnosticDescriptor ImplicitOperatorRule
            => new(DiagnosticId.ExceptionThrownFromImplicitOperator,
                Resources.ExceptionThrownFromImplicitOperatorAnalyzerTitle,
                Resources.ExceptionThrownFromImplicitOperatorAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor PropertyGetterRule
            => new(DiagnosticId.ExceptionThrownFromPropertyGetter,
                Resources.ExceptionThrownFromPropertyGetterAnalyzerTitle,
                Resources.ExceptionThrownFromPropertyGetterAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor StaticConstructorRule
            => new(DiagnosticId.ExceptionThrownFromStaticConstructor,
                Resources.ExceptionThrownFromStaticConstructorAnalyzerTitle,
                Resources.ExceptionThrownFromStaticConstructorAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor FinallyBlockRule
            => new(DiagnosticId.ExceptionThrownFromFinallyBlock,
                Resources.ExceptionThrownFromFinallyBlockAnalyzerTitle,
                Resources.ExceptionThrownFromFinallyBlockAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor EqualityOperatorRule
            => new(DiagnosticId.ExceptionThrownFromEqualityOperator,
                Resources.ExceptionThrownFromEqualityOperatorAnalyzerTitle,
                Resources.ExceptionThrownFromEqualityOperatorAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor DisposeRule
            => new(DiagnosticId.ExceptionThrownFromDispose,
                Resources.ExceptionThrownFromDisposeAnalyzerTitle,
                Resources.ExceptionThrownFromDisposeAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor FinalizerRule
            => new(DiagnosticId.ExceptionThrownFromFinalizer,
                Resources.ExceptionThrownFromFinalizerAnalyzerTitle,
                Resources.ExceptionThrownFromFinalizerAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor GetHashCodeRule
            => new(DiagnosticId.ExceptionThrownFromGetHashCode,
                Resources.ExceptionThrownFromGetHashCodeAnalyzerTitle,
                Resources.ExceptionThrownFromGetHashCodeAnalyzerMessage, Category, Severity, true);

        private static DiagnosticDescriptor EqualsRule
            => new(DiagnosticId.ExceptionThrownFromEquals,
                Resources.ExceptionThrownFromEqualsAnalyzerTitle,
                Resources.ExceptionThrownFromEqualsAnalyzerMessage, Category, Severity, true);

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
            var containingType = context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart).ContainingType;
            var warningLocation = context.Node.GetLocation();

            foreach (var ancestor in context.Node.Ancestors())
            {
                if (ancestor.IsKind(SyntaxKind.MethodDeclaration))
                {
                    var method = (MethodDeclarationSyntax)ancestor;
                    var methodName = method.Identifier.ValueText;

                    if (methodName == "Dispose")
                    {
                        var arity = method.ParameterList.Parameters.Count;
                        context.ReportDiagnostic(Diagnostic.Create(DisposeRule, warningLocation, arity == 0 ? "Dispose()" : "Dispose(bool)", containingType.Name));
                        return;
                    }

                    if (methodName == "GetHashCode")
                    {
                        // Make sure we're dealing with the actual members defined in 'Object' in case they're hidden in a subclass
                        var currentMethodSymbol = context.SemanticModel.GetDeclaredSymbol(method);

                        var objectSymbol = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Object);
                        var objectGetHashCodeSymbol = objectSymbol.GetMembers("GetHashCode").Single();

                        while (currentMethodSymbol.IsOverride)
                        {
                            currentMethodSymbol = currentMethodSymbol.OverriddenMethod;
                        }

                        if (currentMethodSymbol.Equals(objectGetHashCodeSymbol, SymbolEqualityComparer.Default))
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
                        context.ReportDiagnostic(Diagnostic.Create(EqualsRule, warningLocation, method.ParameterList.Parameters[0].Type.ToString(), containingType.Name));
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
                        var firstType = operatorDeclaration.ParameterList.Parameters[0].Type.ToString();
                        var secondType = operatorDeclaration.ParameterList.Parameters[1].Type.ToString();
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
        }
    }
}