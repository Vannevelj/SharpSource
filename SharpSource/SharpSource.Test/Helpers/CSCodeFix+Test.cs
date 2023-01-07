using System;
using System.IO;
using System.Net.Http;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace SharpSource.Test
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, MSTestVerifier>
        {
            private static readonly string SystemPrivateCoreLibPath = typeof(System.Runtime.AmbiguousImplementationException).Assembly.Location;

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

                // After netframework the runtime is split in System.Runtime and System.Private.CoreLib
                // All references appear to return System.Private.CoreLib so we'll have to manually insert the System.Runtime one
                TestState.AdditionalReferences.Add(SystemPrivateCoreLibPath);
                TestState.AdditionalReferences.Add(GetDllDirectory("System.Runtime.dll"));
                TestState.AdditionalReferences.Add(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute).Assembly.Location);
                TestState.AdditionalReferences.Add(typeof(System.Net.Http.HttpClient).Assembly.Location);
                TestState.AdditionalReferences.Add(typeof(IHttpClientFactory).Assembly.Location);
                TestState.AdditionalReferences.Add(typeof(Console).Assembly.Location);
                
                // Initialized with an empty object so the underlying test framework doesn't auto-inject all the netcoreapp3.1 references
                TestState.ReferenceAssemblies = new ReferenceAssemblies("net7.0");


                TestState.OutputKind = OutputKind.WindowsApplication;
                TestState.AnalyzerConfigFiles.Add(("/.globalconfig", @"
is_global = true
end_of_line = lf
"));
            }

            protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
                => diagnostic.Id is not "CS5001" && base.IsCompilerDiagnosticIncluded(diagnostic, compilerDiagnostics);

            private static string GetDllDirectory(string dllName) => Path.Combine(Path.GetDirectoryName(SystemPrivateCoreLibPath) ?? "", dllName);
        }
    }
}