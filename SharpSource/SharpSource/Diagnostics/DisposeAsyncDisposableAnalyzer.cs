using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisposeAsyncDisposableAnalyzer : DiagnosticAnalyzer
{
    public static DiagnosticDescriptor Rule => new(
        DiagnosticId.DisposeAsyncDisposable,
        "An object implements IAsyncDisposable and can be disposed of asynchronously in the context it is used",
        "{0} can be disposed of asynchronously",
        Categories.Performance,
        DiagnosticSeverity.Warning,
        true,
        helpLinkUri: "https://github.com/Vannevelj/SharpSource/blob/master/docs/SS059-DisposeAsyncDisposable.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

        context.RegisterCompilationStartAction(context =>
        {
            var asyncDisposable = context.Compilation.GetTypeByMetadataName("System.IAsyncDisposable");
            if (asyncDisposable is null)
            {
                return;
            }

            context.RegisterOperationAction(c => Analyze(c, asyncDisposable), OperationKind.UsingDeclaration, OperationKind.Using);
        });
    }

    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol asyncDisposable)
    {
        var (isAsynchronous, declarationGroup) = context.Operation switch
        {
            IUsingDeclarationOperation u1 => (u1.IsAsynchronous, u1.DeclarationGroup),
            IUsingOperation u2 => (u2.IsAsynchronous, u2.Resources as IVariableDeclarationGroupOperation),
            _ => default
        };

        if (declarationGroup is null || isAsynchronous)
        {
            return;
        }

        var (surroundingContext, _) = context.Operation.GetSurroundingMethodContext();
        if (surroundingContext is null or { IsAsync: false, Name: not WellKnownMemberNames.TopLevelStatementsEntryPointMethodName })
        {
            return;
        }

        if (context.Operation.IsInsideLockStatement())
        {
            return;
        }

        foreach (var declaration in declarationGroup.Declarations)
        {
            foreach (var declarator in declaration.Declarators)
            {
                var type = declarator.Symbol.Type;
                if (type is not null && type.AllInterfaces.Any(i => i.Equals(asyncDisposable, SymbolEqualityComparer.Default)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), type.Name));
                    return;
                }
            }
        }
    }
}