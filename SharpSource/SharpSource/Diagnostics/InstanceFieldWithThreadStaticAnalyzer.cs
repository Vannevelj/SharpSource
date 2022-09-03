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
public class InstanceFieldWithThreadStaticAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "Field {0} is marked as [ThreadStatic] but is not static";
    private static readonly string Title = "[ThreadStatic] can only be used on static fields";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.InstanceFieldWithThreadStatic, Title, Message, Categories.Correctness, DiagnosticSeverity.Error, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var attribute = fieldDeclaration
            .AttributeLists
            .SelectMany(x => x.Attributes)
            .FirstOrDefault(a => context.SemanticModel.GetSymbolInfo(a.Name).Symbol?.ContainingSymbol.IsType(typeof(ThreadStaticAttribute)) == true);

        if (attribute == default)
        {
            return;
        }

        // const fields are pointless because they can't be changed anyway
        if (fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            var declarators = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            foreach (var declarator in declarators)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, attribute.GetLocation(), declarator.Identifier.ValueText));
            }
        }
    }
}