# CHANGELOG
https://keepachangelog.com/en/1.0.0/

## [1.21.5] - 2023-01-31
- `StringConcatenatedInLoop`: Assignments without a concatenation referencing the assigned field won't trigger a warning
- `StaticInitializerAccessedBeforeInitialization`: No longer triggers if the referenced symbol is used an argument to a method call
- `SwitchIsMissingDefaultLabel`: Restricted the IDE warning to the `switch` value instead of the entire statement
- `EqualsAndGetHashcodeNotImplementedTogether`, `RethrowExceptionWithoutLosingStacktrace` and `StringPlaceHoldersInWrongOrder`: Minor improvements to code fixes to return individual documents instead of a solution

## [1.21.4] - 2023-01-29
- `StringConcatenatedInLoop`: Concatenations inside object creation expressions no longer trigger a warning
- `StringConcatenatedInLoop`: Does not trigger when assigning to the loop variable
- `UnnecessaryEnumerableMaterialization`: Now correctly parses the two subsequent invocations, avoiding issues when they are nested within a method call themselves
- `SwitchDoesNotHandleAllEnumOptions`: Code Fix generates more predictable code when simplification is possible
- `SwitchDoesNotHandleAllEnumOptions`: Performance of Code Fix has been improved

## [1.21.3] - 2023-01-25
- `LoopedRandomInstantiation`: No longer triggers when a seed is passed to the constructor
- `RecursiveOperatorOverload`: No longer triggers multiple identical warnings if the operator is invoked multiple times in the body
- `ExceptionThrownFromProhibitedContext`, `StaticInitializerAccessedBeforeInitialization`, `SynchronousTaskWait`, `UnusedResultOnImmutableObject`, `GetHashCodeRefersToMutableMember`, `RecursiveOperatorOverload`, `StringDotFormatWithDifferentAmountOfArguments` and `EnumWithoutDefaultValue` tests use `VerifyCS`

## [1.21.2] - 2023-01-23
- `AsyncOverloadsAvailable`: Correctly generate the fixed code when conditional access is used, i.e. `writer?.Write()`
- `ElementaryMethodsOfTypeInCollectionNotOverridden`: Now takes overrides in base types into consideration
- `ParameterAssignedInConstructor`: Don't trigger when the referenced member is a constant
- `StringPlaceholdersInWrongOrder`: Code fix generates correct code when there are more than 10 placeholders in the format string
- `UnboundedStackalloc`: Does not trigger when using pointers, e.g. `int*`
- `ThreadSleepInAsyncMethod`: Fixed a rare scenario in which an empty code fix would be offered
- `ElementaryMethodsOfTypeInCollectionNotOverridden`, `LoopedRandomInstantiation`, `OnPropertyChangedWithoutNameOfOperator` and `StringPlaceholdersInWrongOrder` tests use `VerifyCS`

## [1.21.1] - 2023-01-22
- `StringConcatenatedInLoop`: No longer triggers for regular assignments
- `UnnecessaryEnumerableMaterialization`: Now supports conditional access, i.e. `values?.ToArray().ToList()`
- `AsyncOverloadsAvailable`: No longer triggers when contained within a `lock` body
- `AsyncOverloadsAvailable`: Better maintains whitespace, indentation and comments when the code fix is applied
- `AsyncOverloadsAvailable`: Improved the detection of overloads when an optional `CancellationToken` is accepted
- `ExplicitEnumValues` and `EnumWithoutDefaultValue`: Downgraded from warning to info to reduce their prevalence
- `ExceptionThrownFromProhibitedContext`: No longer triggers for `PlatformNotSupportedException`
- `ComparingStringsWithoutStringComparison`: Correctly maintains trailing whitespace when the code fix is applied
- `StringPlaceholdersInWrongOrder` and `UnnecessaryEnumerableMaterialization`: Rewritten to use `IOperation`
- `UnnecessaryEnumerableMaterialization` tests use `VerifyCS`
- `LinqTraversalBeforeFilter`: Fixed typo in diagnostic message

## [1.21.0] - 2023-01-21
- `StringConcatenatedInLoop`: A `string` was concatenated in a loop which introduces intermediate allocations. Consider using a `StringBuilder` or pre-allocated `string` instead.
- `LinqTraversalBeforeFilter`, `LockingOnDiscouragedObject` and `LockingOnMutableReference` use `VerifyCS`

## [1.20.0] - 2023-01-20
- `CollectionManipulatedDuringTraversal`: A collection was modified while it was being iterated over. Make a copy first or avoid iterations while the loop is in progress to avoid an `InvalidOperationException` exception at runtime
- `AccessingTaskResultWithoutAwait`: Now also shows a warning when the method is not `async` but returns a `Task`

## [1.19.1] - 2023-01-14
- `PointlessCollectionToString`: Supports immutable collections
- `StructWithoutElementaryMethodsOverridden`: No longer generates the elementary methods in both declarations if it is a `partial struct`
- `StructWithoutElementaryMethodsOverridden`: Rewritten to use `ISymbol`
- `PointlessCollectionToString`, `ThrowNull`, `ExplicitEnumValues`, `RethrowExceptionWithoutLosingStracktrace`, `SwitchDoesNotHandleAllEnumOptions`, `ThreadSleepInAsyncMethodTests`, `ParameterAssignedInConstructorTests`, `EqualsAndGetHashcodeNotImplemented`, `TestMethodWithoutPublicModifier`, `FlagsEnumValuesAreNotPowersOfTwoTests`, `StructWithoutElementaryMethodsOverridden` and `UnboundedStackalloc` tests use `VerifyCS`

## [1.19.0] - 2023-01-12
- `FormReadSynchronously`: Synchronously accessed `HttpRequest.Form` which uses sync-over-async. Use `HttpRequest.ReadFormAsync()` instead
- `SwitchIsMissingDefaultLabel`: Rewritten using `IOperation` and tests use `VerifyCS`
- `NewtonsoftMixedWithSystemTextJson` tests use `VerifyCS`

## [1.18.0] - 2023-01-10
- `MultipleOrderByCalls`: Successive `OrderBy()` calls will maintain only the last specified sort order. Use `ThenBy()` to combine them
- `TestMethodWithoutTestAttribute`: No longer fires for test constructors
- `TestMethodWithoutTestAttribute`: No longer fires for overridden methods
- `TestMethodWithoutTestAttribute` tests use `VerifyCS`

## [1.17.6] - 2023-01-08
- Switched several test files over to the `VerifyCS` approach

## [1.17.5] - 2023-01-06
- `AsyncMethodWithVoidReturnType` will not trigger for `EventArgs` from other frameworks such as WinUI
- Code fixes that introduce `using` statements will now take "Global Usings" into account and format the `using` statement better

## [1.17.4] - 2023-01-04
- `AsyncOverloadsAvailable`: In a chain of method calls, the code fix no longer replaces the wrong invocation
- `ComparingStringsWithoutStringComparison`: No longer triggers when a culture is passed to `.ToLower()` or `.ToUpper()`
- `ComparingStringsWithoutStringComparison`: Chained method invocations are now supported
- `ComparingStringsWithoutStringComparison`: Rewritten to use `IOperation`

## [1.17.3] - 2023-01-03
- `FlagsEnumValuesAreNotPowersOfTwo`: Code fix now formats correctly and doesn't copy comments
- `HttpContextStoredInField`: Only triggers if a reference to `Microsoft.AspNetCore.Http.IHttpContextAccessor` exists
- `LockingOnMutableReference`: Now includes the name of the field in the error message
- Simplified `GetSyntaxRootAsync` calls in code fixes
- `UnusedResultOnImmutableObject`: Now uses `IOperation`

## [1.17.2] - 2023-01-02
- `SynchronousTaskWait`: Now works for top-level statements
- `SynchronousTaskWait`: Rewritten to use `IOperation`
- `SynchronousTaskWait`: Preserves leading trivia when applying the code fix
- `SynchronousTaskWait`: Doesn't offer a code fix when a timeout is passed to `Task.Wait(timeoutMs)`
- `ComparingStringsWithoutStringComparison`: Preserves leading trivia when applying the code fix
- `ThreadSleepInAsyncMethod`: Rewritten to use `IOperation`
- `ThreadSleepInAsyncMethod`: Supports `async` lambdas

## [1.17.1] - 2023-01-02
- `ElementaryMethodsOfTypeInCollectionNotOverridden`: Fixed an issue where a `NullReferenceException` would be thrown
- `HttpClientInstantiatedDirectly`: Only triggers if a reference to `Microsoft.Extensions.Http` exists and `IHttpClientFactory` is within scope
- `AsyncMethodWithVoidReturnType`: Does not trigger for `interface` and `abstract class` implementations
- `TestMethodWithoutTestAttribute`: Now excludes lifetime hooks such as `Xunit.IAsyncLifetime`
- `TestMethodWithoutTestAttribute`: No longer incorrectly triggers for `[Fact]` attributes inside a `struct`
- `TestMethodWithoutTestAttribute`: Also accepts test methods that return `ValueTask` and `ValueTask<T>`
- `TestMethodWithoutTestAttribute`: Rewritten to use `ISymbol`

## [1.17.0] - 2023-01-01
- `NewtonsoftMixedWithSystemTextJson`: An attempt is made to (de-)serialize an object which combines `System.Text.Json` and `Newtonsoft.Json`. Attributes from one won't be adhered to in the other and should not be mixed.

## [1.16.22] - 2023-01-01
- Added `ConfigureAwait(false)` to all codefixes

## [1.16.21] - 2023-01-01
- `StaticInitializerAccessedBeforeInitialization`: Doesn't trigger an error if a member with the same name but in a different type is referenced
- `TestMethodWithoutPublicModifier`: Now supports custom attributes that inherit from a test attribute

## [1.16.20] - 2023-01-01
- `ElementaryMethodsOfTypeInCollectionNotOverridden`: Fixed a `NullReferenceException` when analysing `extern` declarations

## [1.16.19] - 2023-01-01
- `AsyncOverloadsAvailableAnalyzer`: Analyzer is rewritten to use `IOperation`

## [1.16.18] - 2022-12-31
- Unit tests target .NET 7 and test framework dependencies have been updated
- Vsix build tools dependency is updated

## [1.16.17] - 2022-12-30
- `DivideIntegerByIntegerAnalyzer`: Analyzer is rewritten to use `IOperation`
- `EnumWithoutDefaultValueAnalyzer`: Analyzer is rewritten to use `ISymbol`
- `EqualsAndGetHashcodeNotImplementedTogetherAnalyzer`: Analyzer is rewritten to use `ISymbol`
- `LockingOnDiscouragedObjectAnalyzer`: Analyzer is rewritten to use `IOperation`
- `LockingOnMutableReferenceAnalyzer`: Analyzer is rewritten to use `IOperation`
- `UnnecessaryEnumerableMaterializationAnalyzer`: Use array instead of hash set for searching for small number of elements

## [1.16.16] - 2022-12-30
- `TestMethodWithoutPublicModifier`: Analyzer is rewritten to use `IOperation`

## [1.16.15] - 2022-12-30
- `DateTimeNow`: Correctly handles static imports

## [1.16.14] - 2022-12-30
- Internal code cleanup

## [1.16.13] - 2022-12-30
- `LinqTraversalBeforeFilter`: Analyzer is rewritten to use `IOperation`
- `LinqTraversalBeforeFilter`: Now supports LINQ query syntax

## [1.16.12] - 2022-12-30
- Project is updated to C# 11

## [1.16.11] - 2022-12-30
- Simplified the implementation of all codefixes

## [1.16.10] - 2022-12-30
- `AccessingTaskResultWithoutAwait`: Analyzer is rewritten to use `IOperation`

## [1.16.9] - 2022-12-30
- `InstanceFieldWithThreadStatic`: Simplified the `IsStatic` check

## [1.16.8] - 2022-12-30
- `OnPropertyChangedWithoutNameOf`: Analyzer is rewritten to use `IOperation`

## [1.16.7] - 2022-12-30
- `ThrowNull`: Correctly triggers when a constant or casted `null` value is being thrown

## [1.16.6] - 2022-12-30
- `ThreadStaticWithInitializer` and `InstanceFieldWithThreadStatic`: Analyzer is rewritten to use `IOperation`
- Removed unused test project dependencies
- Split up Test and Build workflows

## [1.16.5] - 2022-12-29
- `DateTimeNow`: No longer incorrectly triggers for `nameof(DateTime.Now)` invocations
- `PointlessCollectionToString`: Correctly handles longer chains with nullable annotations, e.g. `SomeClass.SomeCollection?.ToString()`
- `NewGuid`, `DateTimeNow`, `HttpClientInstantiatedDirectly`, `HttpContextStoredInField`, `ThrowNull`, `PointlessCollectionToString`, `MultipleFromBodyParameters`, `LoopedRandomInstantiation` and `ElementaryMethodsOfTypeInCollectionNotOverridden`: Analyzer is rewritten to use `IOperation`

## [1.16.4] - 2022-12-29
- `ParameterAssignedInConstructor`: Analyzer is rewritten to use `IOperation`

## [1.16.3] - 2022-12-29
- `AsyncMethodWithVoidReturnType` and `AttributeMustSpecifyAttributeUsage`: Analyzer is rewritten to use `IOperation`

## [1.16.2] - 2022-12-29
- `SwitchDoesNotHandleAllEnumOptions`: Analyzer is rewritten to use `IOperation`

## [1.16.1] - 2022-12-28
- `StaticInitializerAccessedBeforeInitialization`: Complete rewrite of the analyzer to use `IOperation`. No functional difference but might be more performant

## [1.16.0] - 2022-12-27
- `PointlessCollectionToString`: `.ToString()` was called on a collection which results in impractical output. Considering using `string.Join()` to display the values instead.

## [1.15.0] - 2022-12-25
- `ThreadStaticWithInitializer`: A field is marked as `[ThreadStatic]` so it cannot contain an initializer. The field initializer is only executed for the first thread.
- `StaticInitializerAccessedBeforeInitialization`: When a reference is part of a lambda expression we no longer incorrectly mark it as an error

## [1.14.1] - 2022-10-16
- `AccessingTaskResultWithoutAwait`: Now also works for top-level functions
- `AccessingTaskResultWithoutAwait`: In null-conditional access scenarios such as `file?.ReadAsync().Result`, invalid code will no longer be suggested by the code fix

## [1.14.0] - 2022-10-16
- `LockingOnMutableReference`: A lock was obtained on a mutable field which can lead to deadlocks when a new value is assigned. Mark the field as `readonly` to prevent re-assignment after a lock is taken.
- `ComparingStringsWithoutStringComparison`: Only suggest one code fix at a time
- `UnusedResultOnImmutableObject`: Don't trigger for custom extension methods on the `string` type

## [1.13.1] - 2022-10-1
- `AsyncOverloadsAvailable`: Correctly suggests passing through a `CancellationToken` if the sync overload accepts one as well

## [1.13.0] - 2022-10-1
- `AsyncOverloadsAvailable`: Now passes through a `CancellationToken` if there is one available in the current context
- `AttributeMustSpecifyAttributeUsage`: Takes definitions on base classes into account
- `ElementaryMethodsOfTypeInCollectionNotOverridden`: Supports `HashSet.Add()` and `Dictionary.Add()`

## [1.12.0] - 2022-09-25
- `ParameterAssignedInConstructor`: A parameter was assigned in a constructor

## [1.11.2] - 2022-09-25
- Fixed an issue where in some scenarios, necessary `using` statements were not getting added
- `StaticInitializerAccessedBeforeInitialization`: no longer triggers when passing a method reference

## [1.11.1] - 2022-09-25
- `SwitchIsMissingDefaultLabel`: code fix now works in top-level statements
- `AttributeMustSpecifyAttributeUsage`: correctly fires when the type is defined in the netstandard assembly

## [1.11.0] - 2022-09-24
- `ComparingStringsWithoutStringComparison`: A `string` is being compared through allocating a new `string`, e.g. using `ToLower()` or `ToUpperInvariant()`. Use a case-insensitive comparison instead which does not allocate.
- `UnnecessaryEnumerableMaterialization`: supports `!.` operator
- `ElementaryMethodsOfTypeInCollectionNotOverridden`: supports `?.` and `!.` operators
- `StaticInitializerAccessedBeforeInitialization`: no longer triggers when referencing itself

## [1.10.1] - 2022-09-16
- `StaticInitializerAccessedBeforeInitialization`: supports implicit object creation expressions
- `NewGuid`: supports implicit object creation expressions
- `HttpClientInstantiatedDirectly`: supports implicit object creation expressions

## [1.10.0] - 2022-09-15
- All analyzers and code fixes now have help codes that link back to the individual documentation
- `StaticInitializerAccessedBeforeInitialization`: don't trigger if the referenced field is marked as `const`

## [1.9.4] - 2022-09-13
- `StaticInitializerAccessedBeforeInitialization`: no longer triggers for `Lazy<T>` invocations when a method is passed as argument
- Added documentation for all analyzers to the repo

## [1.9.3] - 2022-09-12
- Internal code cleanup: all warning messages in the tests are now hardcoded

## [1.9.2] - 2022-09-12
- `StaticInitializerAccessedBeforeInitialization`: now takes `nameof()` usage into account
- `StaticInitializerAccessedBeforeInitialization`: no longer triggers for invocations of `static` functions
- `StaticInitializerAccessedBeforeInitialization`: no longer triggers when the field is of type `Action` or `Func`

## [1.9.1] - 2022-09-12
- Internal code cleanup to remove -Async suffixes on tests

## [1.9.0] - 2022-09-11
- `LinqTraversalBeforeFilter`: An `IEnumerable` extension method was used to traverse the collection and subsequently filtered using `Where()`. If the `Where()` filter is executed first, the traversal will have to iterate over fewer items which will result in better performance.
- `LockingOnDiscouragedObject`: A `lock` was taken using an instance of a discouraged type. `System.String`, `System.Type` and `this` references can all lead to deadlocks and should be replaced with a `System.Object` instance instead.

## [1.8.0] - 2022-09-08
- `StaticInitializerAccessedBeforeInitialization`: A `static` field relies on the value of another `static` field which is defined in the same type. `static` fields are initialized in order of appearance.
- `UnboundedStackalloc`: An array is stack allocated without checking whether the length is within reasonable bounds. This can result in performance degradations and security risks

## [1.7.2] - 2022-09-07
- `AttributeMustSpecifyAttributeUsage`: correctly identify when the attribute has been added so it doesn't continue suggesting the change

## [1.7.1] - 2022-09-06
- `StructWithoutElementaryMethodsOverridden`: take `partial struct` definitions into account where the methods are implemented across separate files
- `TestMethodWithoutTestAttribute`: more accurately exclude `Dispose()` methods

## [1.7.0] - 2022-09-05
- `FlagsEnumValuesAreNotPowersOfTwo` has been rewritten to reduce the scope of its warning. Now it will only warn if a non-negative decimal literal is found which is not a power of two. A code fix will be available if a binary OR expression can be constructed with other enum members
- `FlagsEnumValuesDontFit` will no longer fire as this was inaccurate and already covered by the default CA analyzers
- `FlagsEnumValuesAreNotPowersOfTwo` will now mention the enum member that triggered the violation

## [1.6.0] - 2022-09-04
- `AttributeMustSpecifyAttributeUsage`: warn when an attribute is defined without specifying the `[AttributeUsage]`
- All internal code now uses nullable reference types

## [1.5.0] - 2022-09-04
- `MultipleFromBodyParameters`: warn when an API was defined with multiple `[FromBody]` parameters that attempt to deserialize the request body
- Include README in nuget package

## [1.4.2] - 2022-09-04
- CI will ensure the version has been updated appropriately before releasing a new package
- CI will run its `dotnet format` check much faster

## [1.4.1] - 2022-09-03
- `TestMethodWithoutTestAttribute`: improved the accuracy of discovering `TestClass` and `TestFixture` attributes

## [1.4.0] - 2022-09-03
- `InstanceFieldWithThreadStatic`: warn when `[ThreadStatic]` is applied to an instance field
- Removed `StructShouldNotMutateSelf`
- Restructured the diagnostic categories into _Performance_, _ApiDesign_ and _Correctness_

## [1.3.1] - 2022-09-03
- `ThreadSleepInAsyncMethod` does not suggest a no-op refactor if the method is not marked as `async`

## [1.3.0] - 2022-09-02
- `ElementaryMethodsOfTypeInCollectionNotOverridden` is more targeted and only warns if it finds an actual lookup that will be problematic
- `ExceptionThrownFromProhibitedContext` doesn't crash when encountering empty `throw` statements
- `ExceptionThrownFromProhibitedContext` doesn't crash when encountering `throw` statements that reference properties
- `AsyncOverloadsAvailable` will no longer suggest to use an overload if that overload is the current surrounding method
- `AsyncOverloadsAvailable` now works inside lambda expressions as well
- `UnusedResultOnImmutableObject` doesn't trigger on `CopyTo` and `TryCopyTo`

## [1.2.4] - 2022-08-31
- Fixed: `AsyncOverloadsAvailable` supports methods that return `ValueTask`
- Fixed: `AccessingTaskResultWithoutAwait` supports methods that return `ValueTask`
- Fixed: `ThreadSleepInAsyncMethod` supports methods that return `ValueTask`
- `AsyncMethodWithVoidReturnType` now also works for top-level function declarations and local functions
- `ThreadSleepInAsyncMethod` now also works for top-level function declarations and local functions

## [1.2.3] - 2022-08-29
- Fixed: `GetHashCodeRefersToMutableMember` correctly handles `partial` classes
- Fixed: `EqualsAndGetHashcodeNotImplemented` correctly handles `partial` classes

## [1.2.2] - 2022-08-29
- Fixed: `AsyncOverloadsAvailable` wraps the `await` expression with parentheses when the function return value is accessed inline
- Fixed: `AsyncOverloadsAvailable` no longer suggests a change if it would result in invalid code
- Fixed: `AsyncOverloadsAvailable` now also reports improvements when using top-level statements
- Fixed: `AsyncOverloadsAvailable` takes nullable reference types into account when selecting an overload
- `EqualsAndGetHashcodeNotImplementedTogether` now mentions the class name in the diagnostic message

## [1.2.1] - 2022-08-29
- Fixed: `ElementaryMethodsOfTypeInCollectionNotOverridden` triggers for external types
- Fixed: `ExceptionThrownFromProhibitedContext` will no longer trigger for `NotSupportedException` and `NotImplementedException`
- Fixed: `TestMethodWithoutTestAttribute` no longer crashes when encountering a `record`
- Fixed: `TestMethodWithoutTestAttribute` no longer triggers for `Dispose()` methods

## [1.2.0] - 2022-08-28
- Implemented `UnnecessaryEnumerableMaterialization`: An `IEnumerable` was materialized before a deferred execution call
- `SwitchDoesNotHandleAllEnumOptions` produces more accurate code when static imports cause enum members to conflict
- SharpSource its unit tests now run on .NET 6.0

## [1.1.0] - 2022-08-28
- Implemented `UnusedResultOnImmutableObject`: The result of an operation on an immutable object is unused

## [1.0.0] - 2022-08-28
- Implemented `EnumWithoutDefaultValue`: An enum should specify a default value
- Changed the categories of `ExplicitEnumValues`, `FlagsEnumValuesAreNotPowersOfTwo` and `FlagsEnumValuesDontFit`
- Improved messaging for `DateTimeNow`
- Added documentation

## [0.9.0] - 2022-08-26
- Implemented `HttpContextStoredInField`: show a warning when `HttpContext` was stored in a field. Use `IHttpContextAccessor` instead
- Fixed DiagnosticID of `HttpClientInstantiatedDirectly`

## [0.8.0] - 2022-08-24
- Implemented `HttpClientInstantiatedDirectly`: show a warning when `HttpClient` is instantiated. Use `IHttpClientFactory` instead

## [0.7.0] - 2022-08-23
- Implemented `ExplicitEnumValues`: show a warning when an enum does not explicitly specify its value

## [0.6.0] - 2022-08-15
- Automatically publish updates to Github Packages

## [0.5.0] - 2022-08-15
- Automatically publish updates to the VSIX marketplace

## [0.4.0] - 2022-08-08
- Don't trigger `ElementaryMethodsOfTypeInCollectionNotOverridden` for enums
- Don't trigger `ElementaryMethodsOfTypeInCollectionNotOverridden` for arrays
- `DateTimeNow` now shows the correct code fix title action
