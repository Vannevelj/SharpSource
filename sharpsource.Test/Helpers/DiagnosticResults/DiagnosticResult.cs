using Microsoft.CodeAnalysis;

namespace RoslynTester.DiagnosticResults
{
    /// <summary>
    ///     Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public struct DiagnosticResult
    {
        public DiagnosticResultLocation[] Locations { get; set; }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }
    }
}