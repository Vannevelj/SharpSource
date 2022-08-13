using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StructWithoutElementaryMethodsOverriddenAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.StructsCategory;
        private static readonly string Message = Resources.StructWithoutElementaryMethodsOverriddenAnalyzerMessage;
        private static readonly string Title = Resources.StructWithoutElementaryMethodsOverriddenAnalyzerTitle;

        public static DiagnosticDescriptor Rule
            => new(DiagnosticId.StructWithoutElementaryMethodsOverridden, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.StructDeclaration);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var objectSymbol = context.SemanticModel.Compilation.GetSpecialType(SpecialType.System_Object);
            IMethodSymbol objectEquals = null;
            IMethodSymbol objectGetHashCode = null;
            IMethodSymbol objectToString = null;

            foreach (var symbol in objectSymbol.GetMembers())
            {
                if (!( symbol is IMethodSymbol ))
                {
                    continue;
                }

                var method = (IMethodSymbol)symbol;
                if (method.MetadataName == nameof(Equals) && method.Parameters.Count() == 1)
                {
                    objectEquals = method;
                }

                if (method.MetadataName == nameof(GetHashCode) && !method.Parameters.Any())
                {
                    objectGetHashCode = method;
                }

                if (method.MetadataName == nameof(ToString) && !method.Parameters.Any())
                {
                    objectToString = method;
                }
            }

            var structDeclaration = (StructDeclarationSyntax)context.Node;

            var equalsImplemented = false;
            var getHashCodeImplemented = false;
            var toStringImplemented = false;

            foreach (var node in structDeclaration.Members)
            {
                if (!node.IsKind(SyntaxKind.MethodDeclaration))
                {
                    continue;
                }

                var methodDeclaration = (MethodDeclarationSyntax)node;
                if (!methodDeclaration.Modifiers.Contains(SyntaxKind.OverrideKeyword))
                {
                    continue;
                }

                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration).OverriddenMethod;

                // this will happen if the base class is deleted and there is still a derived class
                if (methodSymbol == null)
                {
                    return;
                }

                while (methodSymbol.IsOverride)
                {
                    methodSymbol = methodSymbol.OverriddenMethod;
                }

                if (methodSymbol.Equals(objectEquals, SymbolEqualityComparer.Default))
                {
                    equalsImplemented = true;
                }

                if (methodSymbol.Equals(objectGetHashCode, SymbolEqualityComparer.Default))
                {
                    getHashCodeImplemented = true;
                }

                if (methodSymbol.Equals(objectToString, SymbolEqualityComparer.Default))
                {
                    toStringImplemented = true;
                }
            }

            if (!equalsImplemented || !getHashCodeImplemented || !toStringImplemented)
            {
                var isEqualsImplemented = new KeyValuePair<string, string>("IsEqualsImplemented", equalsImplemented.ToString());
                var isGetHashcodeImplemented = new KeyValuePair<string, string>("IsGetHashCodeImplemented", getHashCodeImplemented.ToString());
                var isGetToStringImplemented = new KeyValuePair<string, string>("IsToStringImplemented", toStringImplemented.ToString());

                var properties = ImmutableDictionary.CreateRange(new[]
                    {isEqualsImplemented, isGetHashcodeImplemented, isGetToStringImplemented});

                context.ReportDiagnostic(Diagnostic.Create(Rule, structDeclaration.Identifier.GetLocation(), properties, structDeclaration.Identifier));
            }
        }
    }
}