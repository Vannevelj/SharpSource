using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace SharpSource.Test;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
    {
        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                var compilationOptions = solution.GetProject(projectId)!.CompilationOptions!;
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                    compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                return solution;
            });

            TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.Controller).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.IActionResult).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Http.IFormCollection).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(IHttpClientFactory).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Xunit.FactAttribute).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(NUnit.Framework.TestFixtureAttribute).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute).Assembly.Location);
            TestState.AdditionalReferences.Add(typeof(Newtonsoft.Json.JsonSerializer).Assembly.Location);

            // Initialized explicitly so the underlying test framework doesn't auto-inject all the netcoreapp3.1 references
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net60;

            TestState.OutputKind = OutputKind.WindowsApplication;
            TestState.AnalyzerConfigFiles.Add(("/.globalconfig", @"
is_global = true
end_of_line = lf
"));
        }

        protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
            => diagnostic.Id is not "CS5001" && base.IsCompilerDiagnosticIncluded(diagnostic, compilerDiagnostics);
    }
}