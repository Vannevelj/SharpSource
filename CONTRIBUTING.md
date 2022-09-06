# Contributing

## What's involved?
Contributions can come in many forms: filing defects and feature requests, (failing) unit tests, documentations improvements, new analyzers, expanding existing ones, etc.

Remember that this library is not intended to provide productivity helpers or code styling suggestions. It is about **surfacing defects at compile time and preventing issues that would otherwise go unnoticed** until it's too late.

If you have a new feature in mind, try to be as precise as possible. Think about edge cases beforehand and mention them in your post. Restrict the scope of your analyzer if needed so that it only covers a well known defined set of use cases. It is often tempting to implement it in a more open-ended way but ultimately we want to provide a helpful signal-to-noise ratio.

For example, initially `ElementaryMethodsOfTypeInCollectionNotOverridden` would warn every time such a collection was instantiated with such a type. While technically correct it also caused thousands of warnings on large codebases, the vast majority without any actual issues. I've then rewritten the analyzer so that it only warns when an actual lookup is performed. This reduced the warnings from 2000 to 1 but that one warning was actually valuable.

## How do I create a new analyzer?
* Copy the structure of an existing [unit test](https://github.com/Vannevelj/SharpSource/tree/master/SharpSource/SharpSource.Test) file, adjust the names and start writing test cases
* Create a new [`DiagnosticId`](https://github.com/Vannevelj/SharpSource/blob/master/SharpSource/SharpSource/Utilities/DiagnosticId.cs)
* Bump the [package version](https://github.com/Vannevelj/SharpSource/blob/master/SharpSource/SharpSource.Package/SharpSource.Package.csproj#L12)
* Update the [Changelog](https://github.com/Vannevelj/SharpSource/blob/master/CHANGELOG.md)
* Copy an [existing analyzer](https://github.com/Vannevelj/SharpSource/tree/master/SharpSource/SharpSource/Diagnostics) and adjust as necessary
* Optionally: copy an [existing code fix](https://github.com/Vannevelj/SharpSource/tree/master/SharpSource/SharpSource.CodeFixes/Diagnostics) and adjust as necessary
* Run `dotnet format` to fix any formatting issues

If at any time you have questions, create an issue on Github to start a discussion.

## How do I get it released?
CI will run to verify you've added the changelog and bumped the version. Once all tests pass and the code is approved, I'll merge it and a new release will be automatically distributed to all delivery platforms (NuGet, VS marketplace & Github packages).

## Implementation notes
These come in no particular order of importance.

* Unit tests are all written through sample code which exercises (or doesn't) the analyzers. Try to keep the samples as concise as possible. A lot of the older analyzers are quite verbose at times by specifying `namespace` and `class`, even when it's not necessary (e.g. because top-level statements are available). Don't use these as example, go for brevity where it makes sense.
* When deciding on what type of syntax node should trigger your analyzer, approach it in a way that should give the best performance in general. e.g. if you're trying to avoid `throw null;`, don't trigger on `SyntaxKind.NullKeyword` but use `SyntaxKind.ThrowStatement` instead. The former is likely to be much more common than the latter.
* Think about the level you need to work on. Sometimes you need a `SyntaxNode`, sometimes you need an `ISymbol`. In general, the higher level the better. `SyntaxNode` will open you up to dealing with `partial` classes and other implementation details. `ISymbol` abstracts all that so you're less likely to miss some of the edge cases.
* **Expect invalid code**. When typing `void Method() { }`, you'll have many individual moments where the code is in an invalid state: `void M`, `void Method (`, etc. Make sure you include the appropriate `null` checks in your code because they tend to go unnoticed through unit tests as they only include the valid code at the end. Nullable reference types are enabled so that should go a very long way towards preventing these issues.
* Make sure your unit tests cover the less common scenarios as well. Some frequently relevant test cases:
  * `partial` classes and methods
  * top-level statements vs nested in a class
  * Fully qualified names vs imported ones (`System.Attribute` vs `Attribute`)
  * Short-hand attribute names vs long ones (`Obsolete` vs `ObsoleteAttribute`)
  * A declaration with multiple declarators (`int test, test2 = 5;`)
  * Methods with a body vs those with an expression bodied member
  * Methods? Local functions? Global statements? Lambdas? A statement can be contained within many different contexts, don't assume it's all inside a method


## Troubleshooting

### Symbols aren't loaded when running the VSIX
Open your Roslyn hive by running the VSIX. In the Visual Studio environment that opens, go to `Tools > Options > Text Editor > C# > Advanced` and uncheck **Run code analysis in separate process**. Stop the VSIX and try again -- you should now see symbols load and breakpoints will get hit again.