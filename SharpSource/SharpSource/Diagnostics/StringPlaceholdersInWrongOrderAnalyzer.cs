using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StringPlaceholdersInWrongOrderAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.StringPlaceholdersInWrongOrder,
        "Orders the arguments of a string.Format() call in ascending order according to index.",
        "string.Format() Placeholders are not in ascending order.",
        Categories.Correctness,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS015-StringPlaceholdersInWrongOrder.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(context =>
        {
            var stringFormatSymbols = context.Compilation.GetSpecialType(SpecialType.System_String).GetMembers("Format").OfType<IMethodSymbol>().ToImmutableArray();
            context.RegisterOperationAction(context => Analyze(context, stringFormatSymbols), OperationKind.Invocation);
        });
    }

    private static void Analyze(OperationAnalysisContext context, ImmutableArray<IMethodSymbol> stringFormatSymbols)
    {
        var invocation = (IInvocationOperation)context.Operation;

        // Verify we're dealing with a string.Format() call
        if (!stringFormatSymbols.Any(s => invocation.TargetMethod.Equals(s, SymbolEqualityComparer.Default)))
        {
            return;
        }

        // Verify the format is a literal expression and not a method invocation or an identifier
        // The overloads are in the form string.Format(string, object[]) or string.Format(CultureInfo, string, object[])
        if (invocation.Arguments is not { Length: >= 2 })
        {
            return;
        }

        var firstArgument = invocation.Arguments[0];
        var secondArgument = invocation.Arguments[1];

        // Get the formatted string from the correct position
        var (formatArgument, firstArgumentIsLiteral) = (firstArgument.Value, secondArgument.Value) switch
        {
            (ILiteralOperation literal, _) => (literal.ConstantValue, true),
            (_, ILiteralOperation literal) => (literal.ConstantValue, false),
            _ => default
        };

        if (!formatArgument.HasValue)
        {
            return;
        }

        if (formatArgument.Value is not string formatString)
        {
            return;
        }

        // Verify that all placeholders are counting from low to high.
        // Not all placeholders have to be used necessarily, we only re-order the ones that are actually used in the format string.
        //
        // Display a warning when the integers in question are not in ascending or equal order. 
        var placeholders = PlaceholderHelpers.GetPlaceholders(formatString);

        // If there's no placeholder used or there's only one, there's nothing to re-order
        if (placeholders.Count <= 1)
        {
            return;
        }

        for (var index = 1; index < placeholders.Count; index++)
        {
            if (!int.TryParse(placeholders[index - 1].Groups["index"].Value, out var firstValue) ||
                !int.TryParse(placeholders[index].Groups["index"].Value, out var secondValue))
            {
                // Parsing failed
                return;
            }

            // Given a scenario like this:
            //     string.Format("{0} {1} {4} {3}", a, b, c, d)
            // it would otherwise crash because it's trying to access index 4, which we obviously don't have.
            var argumentsToSkip = firstArgumentIsLiteral ? 1 : 2;
            if (firstValue >= invocation.Arguments.Length - argumentsToSkip ||
                secondValue >= invocation.Arguments.Length - argumentsToSkip)
            {
                return;
            }

            // Given a scenario {0} {1} {0} we have to make sure that this doesn't trigger a warning when we're simply re-using an index. 
            // Those are exempt from the "always be ascending or equal" rule.
            bool hasBeenUsedBefore(int value, int currentIndex)
            {
                for (var counter = 0; counter < currentIndex; counter++)
                {
                    if (int.TryParse(placeholders[counter].Groups["index"].Value, out var intValue) && intValue == value)
                    {
                        return true;
                    }
                }

                return false;
            }

            // They should be in ascending or equal order
            if (firstValue > secondValue && !hasBeenUsedBefore(secondValue, index))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation()));
                return;
            }
        }
    }
}