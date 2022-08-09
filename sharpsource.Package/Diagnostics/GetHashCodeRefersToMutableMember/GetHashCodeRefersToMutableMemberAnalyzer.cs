using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Package;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.GetHashCodeRefersToMutableMember
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GetHashCodeRefersToMutableMemberAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.GeneralCategory;
        private static readonly string FieldMessage = Resources.GetHashCodeRefersToMutableFieldAnalyzerFieldMessage;
        private static readonly string PropertyMessage = Resources.GetHashCodeRefersToMutableFieldAnalyzerPropertyMessage;
        private static readonly string Title = Resources.GetHashCodeRefersToMutableFieldAnalyzerTitle;

        public static DiagnosticDescriptor FieldRule => new(DiagnosticId.GetHashCodeRefersToMutableMember, Title, FieldMessage, Category, Severity, true);
        public static DiagnosticDescriptor PropertyRule => new(DiagnosticId.GetHashCodeRefersToMutableMember, Title, PropertyMessage, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(PropertyRule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var declaration = (MethodDeclarationSyntax)context.Node;
            if (declaration?.Identifier == null || declaration.ParameterList == null)
            {
                return;
            }

            if (declaration.Identifier.ValueText != "GetHashCode" || declaration.ParameterList.Parameters.Any())
            {
                return;
            }

            var nodes = declaration.DescendantNodes(descendIntoChildren: target => true);

            var identifierNameNodes = nodes.OfType<IdentifierNameSyntax>(SyntaxKind.IdentifierName);
            foreach (var node in identifierNameNodes)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(node).Symbol;
                if (symbol == null)
                {
                    continue;
                }

                if (symbol.Kind == SymbolKind.Field)
                {
                    var fieldIsMutableOrStatic = FieldIsMutable((IFieldSymbol)symbol);
                    if (fieldIsMutableOrStatic)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(FieldRule, node.GetLocation(), symbol.Name));
                    }
                }
                else if (symbol.Kind == SymbolKind.Property)
                {
                    var root = context.Node.SyntaxTree.GetRoot();
                    var propertyNode = root.FindNode(symbol.Locations[0].SourceSpan);
                    if (propertyNode is PropertyDeclarationSyntax propertyDeclaration)
                    {
                        var propertyIsMutable = PropertyIsMutable((IPropertySymbol)symbol);
                        if (propertyIsMutable)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(PropertyRule, node.GetLocation(), symbol.Name));
                        }
                    }
                }
            }
        }

        private bool FieldIsMutable(IFieldSymbol field)
        {
            if (field.IsConst)
            {
                return false;
            }

            if (field.IsReadOnly && ( field.Type.IsValueType || field.Type.SpecialType == SpecialType.System_String ) && !field.IsStatic)
            {
                return false;
            }

            return true;
        }

        private bool PropertyIsMutable(IPropertySymbol property) => property.SetMethod != null;
    }
}