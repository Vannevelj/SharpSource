# CHANGELOG
https://keepachangelog.com/en/1.0.0/

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
- Implemented `UnnecessaryEnumerableMaterialization`: An IEnumerable was materialized before a deferred execution call
- `SwitchDoesNotHandleAllEnumOptions` produces more accurate code when static imports cause enum members to conflict
- SharpSource its unit tests now run on .NET 6.0

## [1.1.0] - 2022-08-28
- Implemented `UnusedResultOnImmutableObject`: The result of an operation on an immutable object is unused

## [1.0.0] - 2022-08-28
- Implemented `EnumWithoutDefaultValue`: An enum should specify a default value
- Changed the categories of ExplicitEnumValues, FlagsEnumValuesAreNotPowersOfTwo and FlagsEnumValuesDontFit
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