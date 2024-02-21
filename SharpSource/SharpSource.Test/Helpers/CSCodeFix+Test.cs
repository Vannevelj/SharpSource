using System.IO;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
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
        public NullableContextOptions NullableContextOptions { get; set; } = NullableContextOptions.Disable;

        public Test()
        {
            SolutionTransforms.Add((solution, projectId) =>
            {
                var compilationOptions = solution.GetProject(projectId)!.CompilationOptions!;
                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
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
            // Unfortunately MS stopped updating the utility library that abstracted this: https://github.com/dotnet/roslyn-sdk/issues/1047
            //TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.ReferenceAssemblies = new ReferenceAssemblies(
                        "net8.0",
                        new PackageIdentity(
                            "Microsoft.NETCore.App.Ref",
                            "8.0.0"),
                        Path.Combine("ref", "net8.0"));

            TestState.OutputKind = OutputKind.WindowsApplication;
            TestState.AnalyzerConfigFiles.Add(("/.globalconfig", @"
is_global = true
end_of_line = lf
"));
        }

        protected override CompilationOptions CreateCompilationOptions()
            => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true).WithNullableContextOptions(NullableContextOptions);

        protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
            => diagnostic.Id is not "CS5001" && base.IsCompilerDiagnosticIncluded(diagnostic, compilerDiagnostics);
    }
}