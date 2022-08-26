# CHANGELOG
https://keepachangelog.com/en/1.0.0/

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