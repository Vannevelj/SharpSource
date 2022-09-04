using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeMustSpecifyAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "{0} should specify how the attribute can be used";
    private static readonly string Title = "An attribute was defined without specifying the [AttributeUsage]";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.AttributeMustSpecifyAttributeUsage, Title, Message, Categories.Correctness, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null || !classSymbol.InheritsFrom(typeof(Attribute)))
        {
            return;
        }

        var attributeUsage = classDeclaration.AttributeLists.GetAttributesOfType(typeof(AttributeUsageAttribute), context.SemanticModel).FirstOrDefault();
        if (attributeUsage == default)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
        }
    }
}