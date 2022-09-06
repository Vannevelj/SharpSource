[![Nuget Downloads](https://img.shields.io/nuget/dt/SharpSource)](https://www.nuget.org/packages/SharpSource/) [![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/JeroenVannevel.sharpsource)](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource)

This repo houses a collection of analyzers that aim to make some language features and framework types easier to work with. It does this by highlighting when you might be using something incorrectly in a way that would result in suboptimal performance, runtime exceptions or general unintended behaviour. 

In other words, **this repo only contains analyzers for patterns that have a concrete potential to turn into a defect ticket**. It is not intended to help with general housekeeping tasks like formatting your code or providing productivity helpers. 

Interested in contributing? Take a look at [the guidelines](./CONTRIBUTING.md)!

---
 

| Code   | Name  | Description  | Level   | Provides Code Fix?  |
|---|---|---|---|---|
| SS001  | AsyncMethodWithVoidReturnType  | Async methods should return a `Task` to make them awaitable. Without it, execution continues before the asynchronous `Task` has finished an exceptions go unhandled.  | Warning  | Yes  |
| SS002  | DateTimeNow  | Use `DateTime.UtcNow` to get a locale-independent value. `DateTime.Now` uses the system's local timezone which often means unexpected behaviour when working with global teams/deployments.  | Warning  | Yes  |
| SS003  | DivideIntegerByInteger  | The operands of a divisive expression are both integers and result in an implicit rounding.  | Warning  | No  |
| SS004  | ElementaryMethodsOfTypeInCollectionNotOverridden  | Implement `Equals()` and `GetHashcode()` methods for a type used in a collection. Collections use these to fetch objects but by default they use reference equality. Depending on where your objects come from, they might be missed in the lookup.  | Warning  | No  |
| SS005  | EqualsAndGetHashcodeNotImplementedTogether  | Implement `Equals()` and `GetHashcode()` together. Implement both to ensure consistent behaviour around lookups.  | Warning  | Yes  |
| SS006  | ThrowNull  | Throwing `null` will always result in a runtime exception.  | Error  | No  |
| SS007  | FlagsEnumValuesAreNotPowersOfTwo  | `[Flags]` enum members need to be either powers of two, or bitwise OR expressions. This will fire if they are non-negative decimal literals that are not powers of two, and provide a code fix if the value can be achieved through a binary OR using other enum members. | Error  | Yes  |
| SS008  | GetHashCodeRefersToMutableMember  | `GetHashCode(`) refers to mutable or static member. If the object is used in a collection and then is mutated, subsequent lookups will result in a different hash and might cause lookups to fail.   | Warning  | No  |
| SS009  | LoopedRandomInstantiation  | An instance of type `System.Random` is created in a loop. `Random` uses a time-based seed so when used in a fast loop it will end up with multiple identical seeds for subsequent invocations.  | Warning  | No  |
| SS010  | NewGuid  | An empty GUID was created in an ambiguous manner. The default `Guid` constructor creates an instance with an empty value which is rarely what you want.  | Error  | Yes   |
| SS011  | OnPropertyChangedWithoutNameofOperator  | Use the `nameof()` operator in conjunction with `OnPropertyChanged()` to avoid divergence.  | Warning  | Yes  |
| SS012  | RecursiveOperatorOverload  | Recursively using overloaded operator will result in a stack overflow when attempting to use it.  | Error  | No  |
| SS013  | RethrowExceptionWithoutLosingStacktrace | An exception is rethrown in a way that it loses the stacktrace. Use an empty `throw;` statement instead to preserve it.  | Warning  | Yes  |
| SS014  | StringDotFormatWithDifferentAmountOfArguments  | A `string.Format()` call lacks arguments and will cause a runtime exception.  | Error  | Yes  |
| SS015  | StringPlaceholdersInWrongOrder  | Orders the arguments of a `string.Format()` call in ascending order according to index. This reduces the likelihood of the resulting string having data in the wrong place.  | Warning  | Yes  |
| SS017  | StructWithoutElementaryMethodsOverridden  | Structs should implement `Equals()`, `GetHashCode()`, and `ToString()`. By default they use reflection which comes with performance penalties.  | Warning  | Yes  |
| SS018  | SwitchDoesNotHandleAllEnumOptions  | Add cases for missing enum member. That way you won't miss new behaviour in the consuming API since it will be explicitly handled.  | Warning  | Yes  |
| SS019  | SwitchIsMissingDefaultLabel  | Switch is missing a `default` label. Include this to provide fallback behaviour for any missing cases, including when the upstream API adds them later on.  | Warning   | Yes  |
| SS020  | TestMethodWithoutPublicModifier  | Verifies whether a test method has the `public` modifier. Some test frameworks require this to discover unit tests.  | Warning  | Yes  |
| SS021  | TestMethodWithoutTestAttribute  | A method might be missing a test attribute. Helps ensure no unit tests are missing from your test runs.   | Warning  | No  |
| SS022  | ExceptionThrownFromImplicitOperator  | An exception is thrown from an `implicit` operator  | Warning  | No  |
| SS023  | ExceptionThrownFromPropertyGetter  | An exception is thrown from a property getter  |  Warning  | No  |
| SS024  | ExceptionThrownFromStaticConstructor  | An exception is thrown from a `static` constructor  |  Warning  | No  |
| SS025  | ExceptionThrownFromFinallyBlock  | An exception is thrown from a `finally` block  |  Warning  | No  |
| SS026  | ExceptionThrownFromEqualityOperator  | An exception is thrown from an equality operator  |  Warning  | No  |
| SS027  | ExceptionThrownFromDispose   | An exception is thrown from a `Dispose()` method  | Warning  | No  |
| SS028  | ExceptionThrownFromFinalizer  | An exception is thrown from a finalizer method  |  Warning  | No  |
| SS029  | ExceptionThrownFromGetHashCode | An exception is thrown from a `GetHashCode()` method  |  Warning  | No  |
| SS030  | ExceptionThrownFromEquals  | An exception is thrown from an `Equals() method`  |  Warning  | No  |
| SS032  | ThreadSleepInAsyncMethod  | Synchronously sleeping a thread in an `async` method combines two threading models and can lead to deadlocks.  | Warning  | Yes  |
| SS033  | AsyncOverloadsAvailable  | An `async` overload is available. These overloads typically exist to provide better performing IO calls and should generally be preferred.  | Warning  | Yes  |
| SS034  | AccessingTaskResultWithoutAwait  | Use `await` to get the result of an asynchronous operation. While accessing `.Result` is fine once the `Task` has been completed, this removes any ambiguity and helps prevent regressions if the code changes later on.  | Warning  | Yes  |
| SS035  | SynchronousTaskWait  | Asynchronously `await` tasks instead of blocking them to avoid deadlocks.  | Warning  | Yes  |
| SS036  | ExplicitEnumValues  | An enum should explicitly specify its values. Otherwise you risk serializing your enums into different numeric values if you add a new member at any place other than the last line in the enum file.  | Warning  | Yes  |
| SS037  | HttpClientInstantiatedDirectly  | `HttpClient` was instantiated directly. This can result in socket exhaustion and DNS issues in long-running scenarios. Use `IHttpClientFactory` instead.  | Warning  | No  |
| SS038  | HttpContextStoredInField  | `HttpContext` was stored in a field. This can result in a previous context being used for subsequent requests. Use `IHttpContextAccessor` instead.  | Warning  | No  |
| SS039  | EnumWithoutDefaultValue  | An `enum` should specify a default value of 0 as "Unknown" or "None". When an invalid enum value is marshalled or you receive a default value, many systems return it as `0`. This way you don't inadvertedly interpret it as a valid value.  | Warning  | No  |
| SS040  | UnusedResultOnImmutableObject  | The result of an operation on a `string` is unused. At best this has no effect, at worst this means a desired `string` operation has not been performed.  | Warning  | No  |
| SS041  | UnnecessaryEnumerableMaterialization  | An `IEnumerable` was materialized before a deferred execution call. This generally results in unnecessary work being done.  | Warning  | Yes  |
| SS042  | InstanceFieldWithThreadStatic  | `[ThreadStatic]` can only be used on static fields. If used on an instance field the attribute will not have any effect and the subsequent multithreading behaviour will not be as intended.  | Error  | No  |
| SS043  | MultipleFromBodyParameters  | A method specifies multiple `[FromBody]` parameters but only one is allowed. Specify a wrapper type or use `[FromForm]`, `[FromRoute]`, `[FromHeader]` and `[FromQuery]` instead.  | Error  | No  |
| SS044  | AttributeMustSpecifyAttributeUsage  | An attribute was defined without specifying the `[AttributeUsage]`  | Warning  | Yes  |

## Configuration
Is a particular rule not to your liking? There are many ways to adjust their severity and even disable them altogether. For an overview of some of the options, check out [this document](https://docs.microsoft.com/en-gb/dotnet/fundamentals/code-analysis/suppress-warnings).