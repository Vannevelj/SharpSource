using Microsoft.CodeAnalysis;

namespace RoslynTester.Helpers.CSharp
{
    public abstract class CSharpDiagnosticVerifier : DiagnosticVerifier
    {
        public CSharpDiagnosticVerifier() : base(LanguageNames.CSharp)
        {
        }
    }
}