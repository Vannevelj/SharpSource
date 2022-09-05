# CHANGELOG
https://keepachangelog.com/en/1.0.0/

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