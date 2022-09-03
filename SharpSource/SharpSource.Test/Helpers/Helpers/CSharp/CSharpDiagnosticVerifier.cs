using Microsoft.CodeAnalysis;

namespace SharpSource.Test.Helpers.Helpers.CSharp;

public abstract class CSharpDiagnosticVerifier : DiagnosticVerifier
{
    public CSharpDiagnosticVerifier() : base(LanguageNames.CSharp)
    {
    }
}