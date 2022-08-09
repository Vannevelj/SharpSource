using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SharpSource.Package;
using SharpSource.Utilities;

namespace SharpSource.Diagnostics.SwitchIsMissingDefaultLabel
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SwitchIsMissingDefaultLabelAnalyzer : DiagnosticAnalyzer
    {
        private const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        private static readonly string Category = Resources.GeneralCategory;
        private static readonly string Message = Resources.SwitchIsMissingDefaultSectionAnalyzerMessage;
        private static readonly string Title = Resources.SwitchIsMissingDefaultSectionAnalyzerTitle;

        public static DiagnosticDescriptor Rule
            => new(DiagnosticId.SwitchIsMissingDefaultLabel, Title, Message, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.SwitchStatement);
        }

        private void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var switchBlock = (SwitchStatementSyntax)context.Node;

            var hasDefaultLabel = false;
            foreach (var section in switchBlock.Sections)
            {
                foreach (var label in section.Labels)
                {
                    if (label.IsKind(SyntaxKind.DefaultSwitchLabel))
                    {
                        hasDefaultLabel = true;
                    }
                }
            }

            if (!hasDefaultLabel)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, switchBlock.SwitchKeyword.GetLocation()));
            }
        }
    }
}