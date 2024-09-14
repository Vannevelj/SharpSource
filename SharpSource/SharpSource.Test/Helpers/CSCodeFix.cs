using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace SharpSource.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic(int location = 0)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic().WithLocation(location);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor, int location = 0)
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor).WithLocation(location);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string source, string[]? additionalFiles = null, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = source,
        };

        if (additionalFiles != null)
        {
            foreach (var file in additionalFiles)
            {
                var filename = Guid.NewGuid().ToString();
                test.TestState.Sources.Add(($"{filename}.cs", file));
            }
        }

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    public static async Task VerifyNoDiagnostic(string source)
        => await VerifyCodeFix(source, source);

    public static async Task VerifyDiagnosticWithoutFix(string source, params DiagnosticResult[] expected)
        => await VerifyCodeFix(source, expected, source);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
    public static async Task VerifyCodeFix(string source, string fixedSource)
        => await VerifyCodeFix(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
    public static async Task VerifyCodeFix(string source, DiagnosticResult expected, string fixedSource, int codeActionIndex = 0)
        => await VerifyCodeFix(source, new[] { expected }, fixedSource, codeActionIndex);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
    public static async Task VerifyCodeFix(string source, DiagnosticResult[] expected, string fixedSource, int codeActionIndex = 0, string[]? additionalFiles = null)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            CodeActionIndex = codeActionIndex
        };

        if (additionalFiles != null)
        {
            foreach (var file in additionalFiles)
            {
                var filename = Guid.NewGuid().ToString();
                test.TestState.Sources.Add(($"{filename}.cs", file));
                test.FixedState.Sources.Add(($"{filename}.cs", file));
            }
        }

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }
}