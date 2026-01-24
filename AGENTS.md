# SharpSource Repository Context for AI Agents

## Overview
SharpSource is a collection of C# Roslyn analyzers designed to detect code patterns that have concrete potential to become defects. The project focuses on runtime exceptions, performance issues, and unintended behavior rather than code style or formatting.

## Repository Structure
- **Primary Language**: C# targeting .NET Framework 4.7.2, .NET Standard 2.0, and .NET 8
- **Main Projects**:
  - `SharpSource\SharpSource`: Core analyzer implementation
  - `SharpSource\SharpSource.Test`: Unit tests for analyzers
- **Distribution**: NuGet package and Visual Studio Marketplace extension

## Architecture
- **Analyzer Pattern**: Each analyzer inherits from `DiagnosticAnalyzer` with the `[DiagnosticAnalyzer(LanguageNames.CSharp)]` attribute
- **Typical Structure**:
  - Static `DiagnosticDescriptor` properties defining rules
  - `Initialize()` method registering syntax node actions
  - Analysis methods examining specific code patterns
  - Help links point to `docs/` folder in the repository
- **Testing**: Uses Microsoft.CodeAnalysis.Testing framework with pattern `VerifyCS.VerifyDiagnosticWithoutFix()`

## Key Principles
1. **Defect-Focused**: Only includes analyzers for patterns that can lead to actual bugs
2. **Not for Housekeeping**: Excludes formatting, style, or general productivity helpers
3. **Diagnostic IDs**: Follow format `SS###` (e.g., SS008, SS021)
4. **Categories**: Include Correctness, Performance, and API Usage
5. **Help Documentation**: Each analyzer has corresponding markdown documentation in the `docs/` folder

## Diagnostic Metadata
- Diagnostic IDs use `DiagnosticId` utility class
- Categories from `Categories` utility class
- Severity typically `DiagnosticSeverity.Warning`
- All include help link URIs to GitHub documentation

## Test Structure
- Tests verify both positive and negative cases
- Support multiple testing frameworks (MSTest, NUnit, xUnit)
- Use source code strings with diagnostic markers `{|#0:identifier|}`
- Async test methods with `Task` return type

## Writing an analyzer
Generally speaking you want to follow the following pattern:

1. Set up the barebones structure of a new diagnostic (the `DiagnosticAnalyzer` subtype, create the `DiagnosticId`, etc). Just enough so it compiles but doesn't actually do anything. Even if you intend to add a CodeFix, don't add it yet.

2. Set up a new test file for the analyzer and add any test case you can think of. This is where you rely on your knowledge of [the C# language specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/introduction) to tease out edge cases. Particularly useful here are new language features (records, ref structs, static interface members) and lesser used features (partial classes, `stackalloc`, jagged arrays)

3. Implement the analyzer so that all the test cases pass: cases that should trigger it, do and vice versa. Always assert the diagnostic's text against a plain `string`

4. If you have a code fix in mind, update the tests to expect a fix. They will now fail and you must implement the code fix itself.

5. Update the documentation under `docs/`. If a new analyzer and/or code fix was created, create a new markdown file with the correct `DiagnosticId` and descriptive name and follow the existing pattern: reference the correct badges to represent its respective severity and whether or not it has a code fix, provide a description, provide a minimal example of a violation and (if it has a code fix) the corresponding result of the applied fix.

6. Update the `README.md` to include your new analyzer and update `CHANGELOG.md` to prepare for releasing

## Common implementation concerns
* When you need to resolve a type, do it once at the start of compilation rather than on every node or symbol action
* Use the `IOperation` API rather than the `ISyntaxNode` one. Use the `ISymbol` if you work at a semantic level.
* While technically we could support VB.NET, we don't care about it. Don't write any tests for VB.NET
* Use the common helpers as much as possible. You'll find these under `Utilities/Extensions`
* Avoid passing through data from the Analyzer to the CodeFix. Only do this if it would be particularly tedious or expensive to re-calculate the data on the CodeFix side. To do so, you have to pass a "properties" `Dictionary<string, string>` when reporting a diagnostic
* Always report the diagnostic at the smallest scope possible. For example: if you report a diagnostic on a `switch` then you probably want to do it on the `Expression` or a specific `case` label and not the entire `switch` statement
* Favour early returns: the sooner you exit an analyzer, the faster the compiler can move on to the next invocation. Separate your `return` statements to make it easier to track down which unit test (and thus language feature) they're targeting.
* Avoid exceptions at all costs. You want to religiously `null`-check everything because you must assume that code is most frequently in an invalid state during active development.
* When asserting against a diagnostic you must use the special `{|#<num>` and `|}` tags to indicate where in the source code the squiggly lines are shown. The `<num>` is to be replaced with a 0-indexed numeral (0, 1, 2, 3) and represents the index of the diagnostic that is being reported.
* When writing tests, give the test a descriptive name that captures the nuance of the scenario it's testing. Do not hesitate to use long names if it captures the intent better.
* Make sure there are no return statements inside the `RegisterCodeFix` callback - it means the user would see a preview of the same document. Do all precondition checks before this point.

## How to run

* You build the application with `dotnet build`
* You test the application with `dotnet test`