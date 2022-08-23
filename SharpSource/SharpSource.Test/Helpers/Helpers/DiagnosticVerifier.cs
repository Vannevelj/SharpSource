using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using SharpSource.Test.Helpers.DiagnosticResults;
using SharpSource.Test.Helpers.Helpers.Testing;

namespace SharpSource.Test.Helpers.Helpers
{
    /// <summary>
    ///     Superclass of all Unit Tests for DiagnosticAnalyzers
    /// </summary>
    public abstract class DiagnosticVerifier
    {
        private const string CSharpFileExtension = ".cs";

        private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        private static readonly MetadataReference SystemRuntime = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
        private static readonly MetadataReference SystemNetHttp = MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location);
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);

        private const string FileName = "Test";
        private const string FileNameTemplate = FileName + "{0}{1}";
        private const string ProjectName = "TestProject";
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        private readonly string _languageName;

        public DiagnosticVerifier(string languageName)
        {
            _languageName = languageName;
        }

        /// <summary>
        ///     Get the analyzer being tested - to be implemented in non-abstract class
        /// </summary>
        protected abstract DiagnosticAnalyzer DiagnosticAnalyzer { get; }

        /// <summary>
        ///     Helper method to format a Diagnostic into an easily reasible string
        /// </summary>
        /// <param name="analyzer">The analyzer that this Verifer tests</param>
        /// <param name="diagnostics">The Diagnostics to be formatted</param>
        /// <returns>The Diagnostics formatted as a string</returns>
        private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i]);

                var analyzerType = analyzer.GetType();
                var rules = analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                        }
                        else
                        {
                            if (!location.IsInSource)
                            {
                                Assert.Fail($"Test base does not currently handle diagnostics in metadata locations.Diagnostic in metadata:\r\n{diagnostics[i]}");
                            }

                            var resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(CSharpFileExtension) ? "GetCSharpResultAt" : "GetBasicResultAt";
                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                                resultMethodName,
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                analyzerType.Name,
                                rule.Id);
                        }

                        if (i != diagnostics.Length - 1)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                        break;
                    }
                }
            }
            return builder.ToString();
        }

        /// <summary>
        ///     Called to test a DiagnosticAnalyzer when applied on the inputted strings as a source
        ///     Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyDiagnostic(string[] sources, params DiagnosticResult[] expected) => VerifyDiagnostics(sources, _languageName, expected);

        /// <summary>
        ///     Called to test a DiagnosticAnalyzer when applied on the inputted strings as a source
        ///     Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run the analyzers on</param>
        /// <param name="expected">Diagnostic messages that should appear after the analyzer is run on the sources</param>
        protected void VerifyDiagnostic(string[] sources, params string[] expected) => VerifyDiagnostics(sources, _languageName, expected);

        /// <summary>
        ///     Called to test a DiagnosticAnalyzer when applied on the inputted strings as a source
        ///     Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A string representing the document to run the analyzer on</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        protected void VerifyDiagnostic(string source, params DiagnosticResult[] expected) => VerifyDiagnostics(new[] { source }, _languageName, expected);

        /// <summary>
        ///     Called to test a DiagnosticAnalyzer when applied on the inputted strings as a source
        ///     Note: input a DiagnosticResult for each Diagnostic expected
        /// </summary>
        /// <param name="source">A string representing the document to run the analyzer on</param>
        /// <param name="expected">Diagnostic messages that should appear after the analyzer is run on the sources</param>
        protected void VerifyDiagnostic(string source, params string[] expected) => VerifyDiagnostics(new[] { source }, _languageName, expected);

        /// <summary>
        ///     Verifies that no diagnostic is triggered.
        /// </summary>
        /// <param name="source">A string representing the document to run the analyzer on</param>
        protected void VerifyDiagnostic(string source) => VerifyDiagnostics(new[] { source }, _languageName, new string[] { });

        /// <summary>
        ///     Verifies that no diagnostic is triggered.
        /// </summary>
        /// <param name="sources">An array of strings representing the documents</param>
        protected void VerifyDiagnostic(string[] sources) => VerifyDiagnostics(sources, _languageName, new string[] { });

        /// <summary>
        ///     General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
        ///     then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run teh analyzers on</param>
        /// <param name="language">The language of the classes represented by the source strings</param>
        /// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources</param>
        private void VerifyDiagnostics(string[] sources, string language, params DiagnosticResult[] expected)
        {
            var diagnostics = GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer, GetDocuments(sources, language));
            VerifyDiagnosticResults(diagnostics, DiagnosticAnalyzer, expected);
        }

        /// <summary>
        ///     General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
        ///     then verifies each of them.
        /// </summary>
        /// <param name="sources">An array of strings to create source documents from to run teh analyzers on</param>
        /// <param name="language">The language of the classes represented by the source strings</param>
        /// <param name="expected">Diagnostic messages that should appear after the analyzer is run on the sources</param>
        private void VerifyDiagnostics(string[] sources, string language, params string[] expected)
        {
            var diagnostics = GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer, GetDocuments(sources, language));
            VerifyDiagnosticResults(diagnostics, DiagnosticAnalyzer, expected);
        }

        /// <summary>
        ///     Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array
        ///     of expected results.
        ///     Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the
        ///     DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the source code</param>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="expectedResults">Diagnostic results that should have appeared in the code</param>
        private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
        {
            var expectedCount = expectedResults.Count();
            var results = actualResults.ToArray();
            var actualCount = results.Length;

            if (expectedCount != actualCount)
            {
                var diagnosticsOutput = results.Any() ? FormatDiagnostics(analyzer, results) : "NONE.";
                Assert.Fail($"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" actual \"{actualCount}\"\r\n\r\nDiagnostics:\r\n{diagnosticsOutput}\r\n");
            }

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var actual = results[i];
                var expected = expectedResults[i];

                if (actual.Id != expected.Id)
                {
                    Assert.Fail($"Expected diagnostic id to be \"{expected.Id}\" was \"{actual.Id}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, actual)}\r\n");
                }

                if (actual.Severity != expected.Severity)
                {
                    Assert.Fail($"Expected diagnostic severity to be \"{expected.Severity}\" was \"{actual.Severity}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, actual)}\r\n");
                }

                if (actual.GetMessage() != expected.Message)
                {
                    Assert.Fail($"Expected diagnostic message to be \"{expected.Message}\" was \"{actual.GetMessage()}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, actual)}\r\n");
                }

                var actualLocations = new[] { actual.Location }.Concat(actual.AdditionalLocations).ToArray();
                if (actualLocations.Length != expected.Locations.Length)
                {
                    Assert.Fail($"Expected {expected.Locations.Length} locations but got {actualLocations.Length} for Diagnostic:\r\n{FormatDiagnostics(analyzer, actual)}\r\n");
                }

                for (var j = 0; j < expected.Locations.Length; j++)
                {
                    var expectedLocation = expected.Locations[j];
                    var actualLocation = actualLocations[j];
                    VerifyDiagnosticLocation(analyzer, actual, actualLocation, expectedLocation);
                }
            }
        }

        /// <summary>
        ///     Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array
        ///     of expected results.
        ///     Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the
        ///     DiagnosticResult match the actual diagnostic.
        /// </summary>
        /// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the source code</param>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="expectedResults">Diagnostic messages that should have appeared in the code</param>
        private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params string[] expectedResults)
        {
            var expectedCount = expectedResults.Count();
            var results = actualResults.ToArray();
            var actualCount = results.Length;

            if (expectedCount != actualCount)
            {
                var diagnosticsOutput = results.Any() ? FormatDiagnostics(analyzer, results) : "NONE.";
                Assert.Fail($"Mismatch between number of diagnostics returned, expected \"{expectedCount}\" actual \"{actualCount}\"\r\n\r\nDiagnostics:\r\n{diagnosticsOutput}\r\n");
            }

            for (var i = 0; i < expectedResults.Length; i++)
            {
                var actual = results[i];
                var expected = expectedResults[i];

                if (actual.GetMessage() != expected)
                {
                    Assert.Fail($"Expected diagnostic message to be \"{expected}\" was \"{actual.GetMessage()}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, actual)}\r\n");
                }
            }
        }

        /// <summary>
        ///     Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with the location
        ///     in the expected DiagnosticResult.
        /// </summary>
        /// <param name="analyzer">The analyzer that was being run on the sources</param>
        /// <param name="diagnostic">The diagnostic that was found in the code</param>
        /// <param name="actual">The Location of the Diagnostic found in the code</param>
        /// <param name="expected">The DiagnosticResultLocation that should have been found</param>
        private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            if (expected.Line == null || expected.Column == null)
            {
                if (actual != Location.None)
                {
                    Assert.Fail($"Expected:\nA project diagnostic with No location\nActual:\n{FormatDiagnostics(analyzer, diagnostic)}");
                }
            }

            var actualSpan = actual.GetLineSpan();
            Assert.AreEqual(actualSpan.Path, expected.FilePath,
                $"Expected diagnostic to be in file \"{expected.FilePath}\" was actually in file \"{actualSpan.Path}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, diagnostic)}\r\n");

            var actualLinePosition = actualSpan.StartLinePosition;
            if (actualLinePosition.Line + 1 != expected.Line)
            {
                Assert.Fail(
                    $"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, diagnostic)}\r\n");
            }

            if (actualLinePosition.Character + 1 != expected.Column)
            {
                Assert.Fail(
                    $"Expected diagnostic to start at column \"{expected.Column}\" was actually at column \"{actualLinePosition.Character + 1}\"\r\n\r\nDiagnostic:\r\n{FormatDiagnostics(analyzer, diagnostic)}\r\n");
            }
        }

        /// <summary>
        ///     Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
        ///     The returned diagnostics are then ordered by location in the source document.
        /// </summary>
        /// <param name="analyzer">The analyzer to run on the documents</param>
        /// <param name="documents">The Documents that the analyzer will be run on</param>
        /// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
        internal static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, params Document[] documents)
        {
            var diagnostics = new List<Diagnostic>();
            foreach (var project in documents.Select(x => x.Project).Distinct())
            {
                var compilation = project.GetCompilationAsync().Result;

                // We're ignoring the diagnostic that tells us we don't have a main method
                // We're also ignoring the diagnostics that complain about not being able to find a type or namespace
                var systemDiags = compilation.GetDiagnostics()
                        .Where(x => x.Id != "CS5001" && x.Id != "BC30420" && x.Id != "CS0246")
                        .ToList();

                if (systemDiags.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var firstError = systemDiags.First(d => d.Severity == DiagnosticSeverity.Error);
                    throw new InvalidCodeException(
                        $"Unable to compile program: \"{firstError.GetMessage()}\"\n" +
                        $"Error at line {firstError.Location.GetLineSpan().StartLinePosition.Line} and column {firstError.Location.GetLineSpan().StartLinePosition.Character}." +
                        $"{firstError.Location.SourceTree.GetTextAsync().Result}");
                }

                var diags = compilation.WithAnalyzers(ImmutableArray.Create(analyzer)).GetAnalyzerDiagnosticsAsync().Result;
                foreach (var diagnostic in diags)
                {
                    if (diagnostic.Location == Location.None || diagnostic.Location.IsInMetadata)
                    {
                        diagnostics.Add(diagnostic);
                    }
                    else
                    {
                        for (var i = 0; i < documents.Length; i++)
                        {
                            var document = documents[i];
                            var tree = document.GetSyntaxTreeAsync().Result;
                            if (tree == diagnostic.Location.SourceTree)
                            {
                                diagnostics.Add(diagnostic);
                            }
                        }
                    }
                }
            }

            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }

        /// <summary>
        ///     Given an array of strings as sources and a language, turn them into a project and return the documents and spans of
        ///     it.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Tuple containing the Documents produced from the sources and thier TextSpans if relevant</returns>
        private static Document[] GetDocuments(string[] sources, string language)
        {
            if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
            {
                throw new ArgumentException("Unsupported Language");
            }

            var project = CreateProject(sources, language);
            var documents = project.Documents.ToArray();

            if (sources.Length != documents.Length)
            {
                throw new SystemException("Amount of sources did not match amount of Documents created");
            }

            return documents;
        }

        /// <summary>
        ///     Create a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Document created from the source string</returns>
        internal static Document CreateDocument(string source, string language = LanguageNames.CSharp)
            => CreateProject(new[] { source }, language).Documents.First();

        /// <summary>
        ///     Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Project created out of the Douments created from the source strings</returns>
        private static Project CreateProject(IEnumerable<string> sources, string language = LanguageNames.CSharp)
        {
            var extension = CSharpFileExtension;
            var projectId = ProjectId.CreateNewId(ProjectName);

            var csharpReferences = new[]
            {
                CorlibReference,
                SystemCoreReference,
                CSharpSymbolsReference,
                CodeAnalysisReference,
                SystemRuntime,
                SystemNetHttp
            };

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, ProjectName, ProjectName, language)
                .AddMetadataReferences(projectId, csharpReferences);

            var key = new OptionKey(FormattingOptions.NewLine, LanguageNames.CSharp);
            var options = solution.Options.WithChangedOption(key, "\n");
            solution = solution.WithOptions(options);

            var count = 0;
            foreach (var source in sources)
            {
                var newFileName = string.Format(FileNameTemplate, count, extension);
                var documentId = DocumentId.CreateNewId(projectId, newFileName);
                solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
                count++;
            }

            return solution.GetProject(projectId);
        }
    }
}