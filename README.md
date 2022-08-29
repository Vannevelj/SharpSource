[![Nuget Downloads](https://img.shields.io/nuget/dt/SharpSource)](https://www.nuget.org/packages/SharpSource/) [![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/JeroenVannevel.sharpsource)](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource)
 

| Code   | Name  | Description  | Level   | Provides Code Fix?  |
|---|---|---|---|---|
| SS001  | AsyncMethodWithVoidReturnType  | Async methods should return a `Task` to make them awaitable  | Warning  | Yes  |
| SS002  | DateTimeNow  | Use `DateTime.UtcNow` to get a locale-independent value  | Warning  | Yes  |
| SS003  | DivideIntegerByInteger  | The operands of a divisive expression are both integers and result in an implicit rounding  | Warning  | No  |
| SS004  | ElementaryMethodsOfTypeInCollectionNotOverridden  | Implement `Equals()` and `GetHashcode()` methods for a type used in a collection  | Warning  | No  |
| SS005  | EqualsAndGetHashcodeNotImplementedTogether  | Implement `Equals()` and `GetHashcode()` together  | Warning  | Yes  |
| SS006  | ThrowNull  | Throwing `null` will always result in a runtime exception  | Error  | No  |
| SS007  | FlagsEnumValuesAreNotPowersOfTwo  | A `[Flags]` enum its values are not explicit powers of 2  | Error  | Yes  |
| SS008  | GetHashCodeRefersToMutableMember  | `GetHashCode(`) refers to mutable or static member   | Warning  | No  |
| SS009  | LoopedRandomInstantiation  | An instance of type `System.Random` is created in a loop   | Warning  | No  |
| SS010  | NewGuid  | An empty guid was created in an ambiguous manner  | Error  | Yes   |
| SS011  | OnPropertyChangedWithoutNameofOperator  | Use the `nameof()` operator in conjunction with `OnPropertyChanged()`  | Warning  | Yes  |
| SS012  | RecursiveOperatorOverload  | Recursively using overloaded operator  | Error  | No  |
| SS013  | RethrowExceptionWithoutLosingStacktrace | An exception is rethrown in a way that it loses the stacktrace  | Warning  | Yes  |
| SS014  | StringDotFormatWithDifferentAmountOfArguments  | A `string.Format()` call lacks arguments and will cause a runtime exception  | Error  | Yes  |
| SS015  | StringPlaceholdersInWrongOrder  | Orders the arguments of a `string.Format()` call in ascending order according to index  | Warning  | Yes  |
| SS016  | StructShouldNotMutateSelf  | A `struct` replaces `this` with a new instance  | Warning  | No  |
| SS017  | StructWithoutElementaryMethodsOverridden  | Structs should implement `Equals()`, `GetHashCode()`, and `ToString()`  | Warning  | Yes  |
| SS018  | SwitchDoesNotHandleAllEnumOptions  | Add cases for missing enum member  | Warning  | Yes  |
| SS019  | SwitchIsMissingDefaultLabel  | Switch is missing a `default` label  | Warning   | Yes  |
| SS020  | TestMethodWithoutPublicModifier  | Verifies whether a test method has the `public` modifier  | Warning  | Yes  |
| SS021  | TestMethodWithoutTestAttribute  | A method might be missing a test attribute   | Warning  | No  |
| SS022  | ExceptionThrownFromImplicitOperator  | An exception is thrown from an `implicit` operator  | Warning  | No  |
| SS023  | ExceptionThrownFromPropertyGetter  | An exception is thrown from a property getter  |  Warning  | No  |
| SS024  | ExceptionThrownFromStaticConstructor  | An exception is thrown from a `static` constructor  |  Warning  | No  |
| SS025  | ExceptionThrownFromFinallyBlock  | An exception is thrown from a `finally` block  |  Warning  | No  |
| SS026  | ExceptionThrownFromEqualityOperator  | An exception is thrown from an equality operator  |  Warning  | No  |
| SS027  | ExceptionThrownFromDispose   | An exception is thrown from a `Dispose()` method  | Warning  | No  |
| SS028  | ExceptionThrownFromFinalizer  | An exception is thrown from a finalizer method  |  Warning  | No  |
| SS029  | ExceptionThrownFromGetHashCode | An exception is thrown from a `GetHashCode()` method  |  Warning  | No  |
| SS030  | ExceptionThrownFromEquals  | An exception is thrown from an `Equals() method`  |  Warning  | No  |
| SS031  | FlagsEnumValuesDontFit  | A `[Flags]` enum its values are not explicit powers of 2 and its values dont fit in the specified enum type  | Error  | No  |
| SS032  | ThreadSleepInAsyncMethod  | Synchronously sleeping a thread in an `async` method  | Warning  | Yes  |
| SS033  | AsyncOverloadsAvailable  | An `async` overload is available  | Warning  | Yes  |
| SS034  | AccessingTaskResultWithoutAwait  | Use `await` to get the result of an asynchronous operation  | Warning  | Yes  |
| SS035  | SynchronousTaskWait  | Asynchronously await tasks instead of blocking them  | Warning  | Yes  |
| SS036  | ExplicitEnumValues  | An enum should explicitly specify its values  | Warning  | Yes  |
| SS037  | HttpClientInstantiatedDirectly  | `HttpClient` was instantiated directly. Use `IHttpClientFactory` instead  | Warning  | No  |
| SS038  | HttpContextStoredInField  | `HttpContext` was stored in a field. Use `IHttpContextAccessor` instead  | Warning  | No  |
| SS039  | EnumWithoutDefaultValue  | An `enum` should specify a default value of 0 as "Unknown" or "None"  | Warning  | No  |
| SS040  | UnusedResultOnImmutableObject  | The result of an operation on a `string` is unused  | Warning  | No  |
| SS041  | UnnecessaryEnumerableMaterialization  | An `IEnumerable` was materialized before a deferred execution call  | Warning  | Yes  |