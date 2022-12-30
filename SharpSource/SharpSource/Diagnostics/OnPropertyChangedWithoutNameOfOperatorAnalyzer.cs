using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OnPropertyChangedWithoutNameOfOperatorAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.OnPropertyChangedWithoutNameofOperator,
        "Use the nameof() operator in conjunction with OnPropertyChanged()",
        "OnPropertyChanged({0}) can use the nameof() operator.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS011-OnPropertyChangedWithoutNameofOperator.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;


        if (invocation.TargetMethod.Name != "OnPropertyChanged")
        {
            return;
        }

        if (invocation.Arguments.IsEmpty)
        {
            return;
        }

        var invokedProperty = invocation.Arguments[0];
        if (invokedProperty.ArgumentKind != ArgumentKind.Explicit)
        {
            return;
        }

        var argumentValue = invokedProperty.Value;
        while (argumentValue is IParenthesizedOperation parenthesizedOperation)
        {
            argumentValue = parenthesizedOperation.Operand;
        }

        if (argumentValue.Kind == OperationKind.NameOf)
        {
            return;
        }


        if (argumentValue.ConstantValue.Value is not string argumentValueString)
        {
            return;
        }


        var classSymbol = context.ContainingSymbol.ContainingType;
        if (classSymbol == null)
        {
            return;
        }

        foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (string.Equals(property.Name, argumentValueString, StringComparison.OrdinalIgnoreCase))
            {
                var location = argumentValue.Syntax.GetLocation();
                var data = ImmutableDictionary<string, string?>.Empty.Add("parameterName", property.Name);
                context.ReportDiagnostic(Diagnostic.Create(Rule, location, data, property.Name));
            }
        }
    }
}