using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Gets a <see cref="DiagnosticResult"/> without a predefined location.
    /// </summary>
    public static DiagnosticResult DiagnosticWithoutLocation()
        => CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic();

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

    public static async Task VerifyNoDiagnostic(string source, string[]? additionalFiles = null)
        => await VerifyCodeFix(source, DiagnosticResult.EmptyDiagnosticResults, source, additionalFiles: additionalFiles);

    public static async Task VerifyDiagnosticWithoutFix(string source, params DiagnosticResult[] expected)
        => await VerifyCodeFix(source, expected, source);

    public static async Task VerifyDiagnosticWithoutFix(string source, DiagnosticResult expected, string[]? additionalFiles = null)
        => await VerifyCodeFix(source, [expected], source, additionalFiles: additionalFiles);

    public static async Task VerifyDiagnosticWithoutFix(string source, DiagnosticResult expected, Type[] referencesToRemove)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = source,
        };

        RemoveReferences(test, referencesToRemove);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync(CancellationToken.None);
    }

    private static void RemoveReferences(Test test, Type[] referencesToRemove)
    {
        var assemblyLocations = referencesToRemove.Select(t => t.Assembly.Location).ToHashSet();
        var toRemove = test.TestState.AdditionalReferences.Where(r => r.Display is not null && assemblyLocations.Contains(r.Display)).ToList();
        foreach (var reference in toRemove)
        {
            test.TestState.AdditionalReferences.Remove(reference);
        }
    }

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
    public static async Task VerifyCodeFix(string source, DiagnosticResult expected, string fixedSource, int codeActionIndex = 0, string[]? disabledDiagnostics = null)
        => await VerifyCodeFix(source, [expected], fixedSource, codeActionIndex, additionalFiles: null, batchFixedSource: null, disabledDiagnostics);

    /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
    public static async Task VerifyCodeFix(
        string source,
        DiagnosticResult[] expected,
        string fixedSource,
        int codeActionIndex = 0,
        string[]? additionalFiles = null,
        string? batchFixedSource = null,
        string[]? disabledDiagnostics = null,
        int? numberOfIncrementalIterations = null)
    {
        var test = new Test
        {
            TestCode = source,
            FixedCode = fixedSource,
            BatchFixedCode = batchFixedSource!,
            CodeActionIndex = codeActionIndex,
            NumberOfIncrementalIterations = numberOfIncrementalIterations,
        };

        if (disabledDiagnostics != null)
        {
            test.DisabledDiagnostics.AddRange(disabledDiagnostics);
        }

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