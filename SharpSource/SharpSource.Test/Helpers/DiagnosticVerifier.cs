using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using SharpSource.Test.Helpers.Helpers.Testing;

namespace SharpSource.Test.Helpers;

/// <summary>
///     Superclass of all Unit Tests for DiagnosticAnalyzers
/// </summary>
public abstract class DiagnosticVerifier
{
    private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
    // After netframework the runtime is split in System.Runtime and System.Private.CoreLib
    // All references appear to return System.Private.CoreLib so we'll have to manually insert the System.Runtime one
    private static readonly string SystemPrivateCoreLibPath = typeof(AmbiguousImplementationException).Assembly.Location;
    private static readonly MetadataReference SystemRuntime = MetadataReference.CreateFromFile(GetDllDirectory("System.Runtime.dll"));
    private static readonly MetadataReference SystemConsole = MetadataReference.CreateFromFile(typeof(Console).Assembly.Location);
    private static readonly MetadataReference SystemCollections = MetadataReference.CreateFromFile(GetDllDirectory("System.Collections.dll"));
    private static readonly MetadataReference SystemObjectModel = MetadataReference.CreateFromFile(GetDllDirectory("System.ObjectModel.dll"));
    private static readonly MetadataReference SystemNetHttp = MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location);
    private static readonly MetadataReference AspNetCoreHttp = MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Http.HttpContext).Assembly.Location);
    private static readonly MetadataReference AspNetCoreMvc = MetadataReference.CreateFromFile(typeof(Microsoft.AspNetCore.Mvc.FromBodyAttribute).Assembly.Location);
    private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
    private static readonly MetadataReference XunitReference = MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location);
    private static readonly MetadataReference NunitReference = MetadataReference.CreateFromFile(typeof(NUnit.Framework.TestFixtureAttribute).Assembly.Location);
    private static readonly MetadataReference MsTestReference = MetadataReference.CreateFromFile(typeof(Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute).Assembly.Location);
    private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);

    private static string GetDllDirectory(string dllName) => Path.Combine(Path.GetDirectoryName(SystemPrivateCoreLibPath) ?? "", dllName);

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

                        var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                        builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                            "GetCSharpResultAt",
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
    /// <param name="source">A string representing the document to run the analyzer on</param>
    /// <param name="expected">Diagnostic messages that should appear after the analyzer is run on the sources</param>
    protected void VerifyDiagnostic(string source, params string[] expected) => VerifyDiagnostic(new[] { source }, expected);

    /// <summary>
    ///     General method that gets a collection of actual diagnostics found in the source after the analyzer is run,
    ///     then verifies each of them.
    /// </summary>
    /// <param name="sources">An array of strings to create source documents from to run teh analyzers on</param>
    /// <param name="expected">Diagnostic messages that should appear after the analyzer is run on the sources</param>
    protected void VerifyDiagnostic(string[] sources, params string[] expected)
    {
        var diagnostics = GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer, GetDocuments(sources));
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
    /// <param name="expectedResults">Diagnostic messages that should have appeared in the code</param>
    private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params string[] expectedResults)
    {
        var expectedCount = expectedResults.Length;
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
            if (compilation == default)
            {
                throw new InvalidOperationException("No compilation available");
            }

            // We're ignoring the diagnostic that tells us we don't have a main method
            var systemDiags = compilation.GetDiagnostics()
                    .Where(x => x.Id != "CS5001")
                    .ToList();

            if (systemDiags.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                var firstError = systemDiags.First(d => d.Severity == DiagnosticSeverity.Error);
                throw new InvalidCodeException(
                    $"Unable to compile program: \"{firstError.GetMessage()}\"\n" +
                    $"Error at line {firstError.Location.GetLineSpan().StartLinePosition.Line} and column {firstError.Location.GetLineSpan().StartLinePosition.Character}." +
                    $"{firstError.Location.SourceTree?.GetTextAsync().Result}");
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
    ///     Given an array of strings as sources, turn them into a project and return the documents and spans of
    ///     it.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <returns>A Tuple containing the Documents produced from the sources and thier TextSpans if relevant</returns>
    private static Document[] GetDocuments(string[] sources)
    {
        var project = CreateProject(sources);
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
    /// <returns>A Document created from the source string</returns>
    internal static Document CreateDocument(string source) => CreateProject(new[] { source }).Documents.First();

    /// <summary>
    ///     Create a project using the inputted strings as sources.
    /// </summary>
    /// <param name="sources">Classes in the form of strings</param>
    /// <returns>A Project created out of the Douments created from the source strings</returns>
    private static Project CreateProject(IEnumerable<string> sources)
    {
        var projectName = "TestProject";
        var projectId = ProjectId.CreateNewId(projectName);

        var csharpReferences = new[]
        {
            CorlibReference,
            SystemCoreReference,
            CSharpSymbolsReference,
            CodeAnalysisReference,
            SystemRuntime,
            SystemCollections,
            SystemConsole,
            SystemNetHttp,
            SystemObjectModel,
            AspNetCoreHttp,
            AspNetCoreMvc,
            XunitReference,
            MsTestReference,
            NunitReference
        };

        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
            .AddMetadataReferences(projectId, csharpReferences);

        var key = new OptionKey(FormattingOptions.NewLine, LanguageNames.CSharp);
        var options = solution.Options.WithChangedOption(key, "\n");
        solution = solution.WithOptions(options);

        var count = 0;
        foreach (var source in sources)
        {
            var newFileName = $"Test{count}.cs";
            var documentId = DocumentId.CreateNewId(projectId, newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
            count++;
        }

        var newProject = solution.GetProject(projectId);
        if (newProject == default)
        {
            throw new InvalidOperationException("Unable to create new project");
        }
        return newProject;
    }

    /// <summary>
    ///     Returns the codefix being tested - to be implemented in non-abstract class
    /// </summary>
    /// <returns>The CodeFixProvider to be used</returns>
    protected virtual CodeFixProvider? CodeFixProvider { get; }

    internal async void VerifyFix(string oldSource, string newSource, int? codeFixIndex = null, params string[] allowedNewCompilerDiagnosticsId)
    {
        if (allowedNewCompilerDiagnosticsId == null || !allowedNewCompilerDiagnosticsId.Any())
        {
            VerifyFix(oldSource, newSource, codeFixIndex, false);
        }
        else
        {
            var document = CreateDocument(oldSource);
            var compilerDiagnostics = GetCompilerDiagnostics(document).ToArray();

            VerifyFix(oldSource, newSource, codeFixIndex, true);

            var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document)).ToList();

            if (newCompilerDiagnostics.Any(diagnostic => allowedNewCompilerDiagnosticsId.Any(s => s == diagnostic.Id)))
            {
                var root = await document.GetSyntaxRootAsync();
                Assert.Fail(
                    "Fix introduced new compiler diagnostics. " +
                    $"\r\n{root?.ToFullString()}" +
                    $"\r\n\r\n{string.Join(Environment.NewLine, newCompilerDiagnostics.Select(d => d.ToString()))}");
            }
        }
    }

    /// <summary>
    ///     General verifier for codefixes.
    ///     Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
    ///     Then gets the string after the codefix is applied and compares it with the expected result.
    ///     Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to
    ///     true.
    /// </summary>
    /// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
    /// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
    /// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
    /// <param name="allowNewCompilerDiagnostics">
    ///     A bool controlling whether or not the test will fail if the CodeFix
    ///     introduces other warnings after being applied
    /// </param>
    private void VerifyFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
    {
        if (DiagnosticAnalyzer == null)
        {
            throw new ArgumentNullException(nameof(DiagnosticAnalyzer));
        }

        if (CodeFixProvider == null)
        {
            throw new ArgumentNullException(nameof(CodeFixProvider));
        }

        var document = CreateDocument(oldSource);
        var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer, document);
        var compilerDiagnostics = GetCompilerDiagnostics(document).ToArray();
        var attempts = analyzerDiagnostics.Length;

        for (var i = 0; i < attempts; ++i)
        {
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
            CodeFixProvider.RegisterCodeFixesAsync(context).Wait();

            if (!actions.Any())
            {
                break;
            }

            if (codeFixIndex != null)
            {
                document = ApplyFix(document, actions.ElementAt(codeFixIndex.Value));
                break;
            }

            document = ApplyFix(document, actions.ElementAt(0));
            analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer, document);

            var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));
            var interestingDiagnostics = newCompilerDiagnostics.Where(x => x.Id != "CS8019");

            //check if applying the code fix introduced any new compiler diagnostics
            if (!allowNewCompilerDiagnostics && interestingDiagnostics.Any())
            {
                var root = document.GetSyntaxRootAsync().Result;
                if (root == default)
                {
                    throw new InvalidOperationException("Failed to retrieve syntax root");
                }
                // Format and get the compiler diagnostics again so that the locations make sense in the output
                document = document.WithSyntaxRoot(Formatter.Format(root, Formatter.Annotation, document.Project.Solution.Workspace));
                interestingDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                Assert.Fail(
                    "Fix introduced new compiler diagnostics. " +
                    $"\r\n{root.ToFullString()}" +
                    $"\r\n\r\n{string.Join(Environment.NewLine, interestingDiagnostics.Select(d => d.ToString()))}");
            }

            //check if there are analyzer diagnostics left after the code fix
            if (!analyzerDiagnostics.Any())
            {
                break;
            }
        }

        //after applying all of the code fixes, compare the resulting string to the inputted one
        var actual = GetStringFromDocument(document);
        Assert.AreEqual(newSource, actual, "Expected document is not the same as the resulting one.");
    }

    /// <summary>
    ///     Apply the inputted CodeAction to the inputted document.
    ///     Meant to be used to apply codefixes.
    /// </summary>
    /// <param name="document">The Document to apply the fix on</param>
    /// <param name="codeAction">A CodeAction that will be applied to the Document.</param>
    /// <returns>A Document with the changes from the CodeAction</returns>
    private static Document ApplyFix(Document document, CodeAction codeAction)
    {
        var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
        var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var newDocument = solution.GetDocument(document.Id);
        if (newDocument == default)
        {
            throw new InvalidOperationException("Failed to fetch document after applying fixes");
        }
        return newDocument;
    }

    /// <summary>
    ///     Compare two collections of Diagnostics,and return a list of any new diagnostics that appear only in the second
    ///     collection.
    ///     Note: Considers Diagnostics to be the same if they have the same Ids.  In teh case of mulitple diagnostics with the
    ///     smae Id in a row,
    ///     this method may not necessarily return the new one.
    /// </summary>
    /// <param name="diagnostics">The Diagnostics that existed in the code before the CodeFix was applied</param>
    /// <param name="newDiagnostics">The Diagnostics that exist in the code after the CodeFix was applied</param>
    /// <returns>A list of Diagnostics that only surfaced in the code after the CodeFix was applied</returns>
    private static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
    {
        var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

        var oldIndex = 0;
        var newIndex = 0;

        while (newIndex < newArray.Length)
        {
            if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
            {
                ++oldIndex;
                ++newIndex;
            }
            else
            {
                yield return newArray[newIndex++];
            }
        }
    }

    /// <summary>
    ///     Get the existing compiler diagnostics on the inputted document.
    /// </summary>
    /// <param name="document">The Document to run the compiler diagnostic analyzers on</param>
    /// <returns>The compiler diagnostics that were found in the code</returns>
    private static IEnumerable<Diagnostic> GetCompilerDiagnostics(Document document) =>
        document.GetSemanticModelAsync()?.Result?.GetDiagnostics() ?? Enumerable.Empty<Diagnostic>();

    /// <summary>
    ///     Given a document, turn it into a string based on the syntax root
    /// </summary>
    /// <param name="document">The Document to be converted to a string</param>
    /// <returns>A string contianing the syntax of the Document after formatting</returns>
    private static string GetStringFromDocument(Document? document)
    {
        if (document == default)
        {
            return "";
        }

        var simplifiedDoc = Simplifier.ReduceAsync(document, Simplifier.Annotation).Result;
        var root = simplifiedDoc.GetSyntaxRootAsync().Result;
        if (root == default)
        {
            throw new InvalidOperationException("Failed to fetch root");
        }
        root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
        return root.GetText().ToString();
    }
}