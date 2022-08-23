using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnPropertyChangedWithoutNameOfOperatorAnalyzer : DiagnosticAnalyzer
{
    private static readonly string Message = "OnPropertyChanged({0}) can use the nameof() operator.";
    private static readonly string Title = "Use the nameof() operator in conjunction with OnPropertyChanged()";

    public static DiagnosticDescriptor Rule => new(DiagnosticId.OnPropertyChangedWithoutNameofOperator, Title, Message, Categories.General, DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not IdentifierNameSyntax identifierExpression)
        {
            return;
        }

        var identifier = identifierExpression.Identifier;
        if (identifier.ValueText != "OnPropertyChanged")
        {
            return;
        }

        if (invocation.ArgumentList == null || !invocation.ArgumentList.Arguments.Any())
        {
            return;
        }

        var invokedProperty = invocation.ArgumentList.Arguments.FirstOrDefault();
        if (invokedProperty == null)
        {
            return;
        }

        // We use the descendent nodes in case it's wrapped in another level. For example: OnPropertyChanged(((nameof(MyProperty))))
        foreach (var expression in invokedProperty.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(SyntaxKind.InvocationExpression))
        {
            if (expression.IsNameofInvocation())
            {
                return;
            }
        }

        var invocationArgument = context.SemanticModel.GetConstantValue(invokedProperty.Expression);
        if (!invocationArgument.HasValue)
        {
            return;
        }

        // Get all the properties defined in this type
        // We can't just get all the descendents of the classdeclaration because that would pass by some of a partial class' properties
        var classDeclaration = invocation.Ancestors().OfType<ClassDeclarationSyntax>(SyntaxKind.ClassDeclaration).FirstOrDefault();
        if (classDeclaration == null)
        {
            return;
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null)
        {
            return;
        }

        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (string.Equals(property.Name, (string)invocationArgument.Value, StringComparison.OrdinalIgnoreCase))
            {
                // The original Linq was `Last()`.  I used `LastOrDefault()` just because I didn't feel the need to implement a
                // version to throw an `InvalidOperationException()` rather than a `NullReferenceException()` in this case.
                var location = invokedProperty.Expression.DescendantNodesAndSelf().LastOrDefault().GetLocation();
                var data = ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<string, string>("parameterName", property.Name),
                    new KeyValuePair<string, string>("startLocation", location.SourceSpan.Start.ToString(CultureInfo.InvariantCulture))
                });
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, data, property.Name));
            }
        }
    }
}