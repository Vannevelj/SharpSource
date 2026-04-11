[![Nuget Downloads](https://img.shields.io/nuget/dt/SharpSource)](https://www.nuget.org/packages/SharpSource/) [![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/JeroenVannevel.sharpsource)](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource)

![Demonstration of an analyzer which removes unnecessary IEnumerable materializations](https://user-images.githubusercontent.com/2777107/190013653-edfcb61b-06a1-46d4-8b99-a71173beebb2.gif)

## Quickstart

Install it through the command line:

```powershell
Install-Package SharpSource
```

or add a reference yourself:

```xml
<ItemGroup>
    <PackageReference Include="SharpSource" Version="1.33.1" PrivateAssets="All" />
</ItemGroup>
```

If you would like to install it as an extension instead, download it [from the marketplace](https://marketplace.visualstudio.com/items?itemName=JeroenVannevel.sharpsource).

---

This repo houses a collection of analyzers that aim to make some language features and framework types easier to work with. It does this by highlighting when you might be using something incorrectly in a way that would result in suboptimal performance, runtime exceptions or general unintended behaviour. 

In other words, **this repo only contains analyzers for patterns that have a concrete potential to turn into a defect ticket**. It is not intended to help with general housekeeping tasks like formatting your code or providing productivity helpers. 

Interested in contributing? Take a look at [the guidelines](./CONTRIBUTING.md)!

---

Detailed explanations of each analyzer can be found in the documentation: https://github.com/Vannevelj/SharpSource/tree/master/docs
 

| Code   | Name | Description |
|--------|------|-------------|
| SS001  | AsyncMethodWithVoidReturnType | Async methods should return `Task` instead of `void` to allow proper exception handling and awaiting. |
| SS002  | DateTimeNow | Use `DateTime.UtcNow` instead of `DateTime.Now` to avoid timezone-related issues. |
| SS003  | DivideIntegerByInteger | Dividing integers results in integer division; cast to floating-point if a decimal result is expected. |
| SS004  | ElementaryMethodsOfTypeInCollectionNotOverridden | Types used as collection keys should override `Equals` and `GetHashCode`. |
| SS005  | EqualsAndGetHashcodeNotImplementedTogether | `Equals` and `GetHashCode` should always be overridden together. |
| SS006  | ThrowNull | Throwing `null` will result in a `NullReferenceException` at runtime. |
| SS007  | FlagsEnumValuesAreNotPowersOfTwo | Flags enum values should be powers of two to allow proper bitwise operations. |
| SS008  | GetHashCodeRefersToMutableMember | `GetHashCode` should not reference mutable members as this breaks hash-based collections. |
| SS009  | LoopedRandomInstantiation | Creating `Random` instances in a loop can produce identical sequences; reuse a single instance. |
| SS010  | NewGuid | Use `Guid.NewGuid()` instead of `new Guid()` to generate a unique identifier. |
| SS011  | OnPropertyChangedWithoutNameofOperator | Use `nameof()` instead of hardcoded property name strings in `OnPropertyChanged`. |
| SS012  | RecursiveOperatorOverload | Operator overloads calling themselves will cause infinite recursion. |
| SS013  | RethrowExceptionWithoutLosingStacktrace | Use `throw;` instead of `throw ex;` to preserve the original stack trace. |
| SS014  | StringDotFormatWithDifferentAmountOfArguments | The number of format placeholders should match the number of arguments. |
| SS015  | StringPlaceholdersInWrongOrder | Format placeholders should be in sequential order starting from `{0}`. |
| SS017  | StructWithoutElementaryMethodsOverridden | Structs should override `Equals`, `GetHashCode`, and implement `IEquatable<T>` for performance. |
| SS018  | SwitchDoesNotHandleAllEnumOptions | Switch statements on enums should handle all possible values. |
| SS019  | SwitchIsMissingDefaultLabel | Switch statements should have a default case to handle unexpected values. |
| SS020  | TestMethodWithoutPublicModifier | Test methods must be public to be discovered by test runners. |
| SS021  | TestMethodWithoutTestAttribute | Methods that look like tests should have a test attribute to be executed. |
| SS022  | ExceptionThrownFromImplicitOperator | Implicit operators should not throw exceptions as they are called invisibly. |
| SS023  | ExceptionThrownFromPropertyGetter | Property getters should not throw exceptions; consider using a method instead. |
| SS024  | ExceptionThrownFromStaticConstructor | Exceptions in static constructors cause `TypeInitializationException` and make the type unusable. |
| SS025  | ExceptionThrownFromFinallyBlock | Exceptions in finally blocks can mask original exceptions from try blocks. |
| SS026  | ExceptionThrownFromEqualityOperator | Equality operators should not throw exceptions; return `false` for invalid comparisons. |
| SS027  | ExceptionThrownFromDispose | `Dispose` methods should not throw exceptions as they may be called during exception unwinding. |
| SS028  | ExceptionThrownFromFinalizer | Finalizers should not throw exceptions as this will terminate the process. |
| SS029  | ExceptionThrownFromGetHashCode | `GetHashCode` should not throw exceptions; return a consistent value instead. |
| SS030  | ExceptionThrownFromEquals | `Equals` should not throw exceptions; return `false` for invalid comparisons. |
| SS032  | ThreadSleepInAsyncMethod | Use `await Task.Delay()` instead of `Thread.Sleep()` in async methods to avoid blocking threads. |
| SS033  | AsyncOverloadsAvailable | Use async overloads when available to avoid blocking the calling thread. |
| SS034  | AccessingTaskResultWithoutAwait | Accessing `Task.Result` without awaiting can cause deadlocks; use `await` instead. |
| SS035  | SynchronousTaskWait | Using `.Wait()` or `.Result` on tasks can cause deadlocks; use `await` instead. |
| SS036  | ExplicitEnumValues | Enum members should have explicit values when the values are persisted or serialized. |
| SS037  | HttpClientInstantiatedDirectly | Use `IHttpClientFactory` instead of creating `HttpClient` directly to avoid socket exhaustion. |
| SS038  | HttpContextStoredInField | `HttpContext` should not be stored in fields as it's request-scoped and may be invalid later. |
| SS039  | EnumWithoutDefaultValue | Enums should have a member with value `0` to represent the default state. |
| SS040  | UnusedResultOnImmutableObject | Methods on immutable types return new instances; the result should not be discarded. |
| SS041  | UnnecessaryEnumerableMaterialization | Avoid materializing enumerables (e.g., `ToList()`) when the result is immediately enumerated. |
| SS042  | InstanceFieldWithThreadStatic | `[ThreadStatic]` only works on static fields; it has no effect on instance fields. |
| SS043  | MultipleFromBodyParameters | Web API actions can only have one `[FromBody]` parameter. |
| SS044  | AttributeMustSpecifyAttributeUsage | Custom attributes should specify `[AttributeUsage]` to define valid targets. |
| SS045  | StaticInitializerAccessedBeforeInitialization | Static field initializers may access fields before they are initialized. |
| SS046  | UnboundedStackalloc | `stackalloc` without a size limit can cause stack overflow; consider using a maximum size. |
| SS047  | LinqTraversalBeforeFilter | Apply `Where` filters before `Select` projections to avoid unnecessary work. |
| SS048  | LockingOnDiscouragedObject | Avoid locking on `this`, `typeof()`, or strings as these can cause deadlocks. |
| SS049  | ComparingStringsWithoutStringComparison | String comparisons should specify a `StringComparison` to ensure correct behavior. |
| SS050  | ParameterAssignedInConstructor | Assigning to a parameter instead of a field in a constructor is likely a mistake. |
| SS051  | LockingOnMutableReference | Locking on a field that can be reassigned may cause race conditions. |
| SS052  | ThreadStaticWithInitializer | `[ThreadStatic]` field initializers only run once; use lazy initialization instead. |
| SS053  | PointlessCollectionToString | Calling `ToString()` on collections returns the type name, not the contents. |
| SS054  | NewtonsoftMixedWithSystemTextJson | Mixing Newtonsoft.Json and System.Text.Json attributes causes serialization issues. |
| SS055  | MultipleOrderByCalls | Multiple `OrderBy` calls override each other; use `ThenBy` for secondary sorting. |
| SS056  | FormReadSynchronously | Reading form data synchronously blocks threads; use async methods instead. |
| SS057  | CollectionManipulatedDuringTraversal | Modifying a collection while iterating over it causes `InvalidOperationException`. |
| SS058  | StringConcatenatedInLoop | Use `StringBuilder` instead of string concatenation in loops for better performance. |
| SS059  | DisposeAsyncDisposable | Types implementing `IAsyncDisposable` should be disposed with `await using`. |
| SS060  | ConcurrentDictionaryEmptyCheck | Use `IsEmpty` instead of `Count == 0` on `ConcurrentDictionary` for thread safety. |
| SS061  | ImmutableCollectionCreatedIncorrectly | Use builder methods or `Create()` instead of constructors for immutable collections. |
| SS062  | ActivityWasNotStopped | `Activity` instances must be stopped to ensure telemetry data is recorded. |
| SS063  | ValueTaskAwaitedMultipleTimes | `ValueTask` can only be awaited once; store the result or convert to `Task` if needed. |
| SS064  | UnnecessaryToStringOnSpan | Avoid calling `ToString()` on spans when an overload accepting spans directly is available. |
| SS065  | LoggerMessageAttribute | Use the `[LoggerMessage]` attribute for high-performance logging instead of extension methods. |
| SS066  | DisposableFieldIsNotDisposed | Disposable fields owned by a type should be included in its disposal path to avoid resource leaks. |
| SS067  | RedisResponseNotHandled | Redis responses must be checked for errors since these clients do not throw exceptions. |
| SS068  | TimeSpanConstructedWithTicks | `TimeSpan` single-parameter constructor creates ticks (100ns), not seconds. Use `TimeSpan.FromTickets()` to avoid confusion. |

## Configuration
Is a particular rule not to your liking? There are many ways to adjust their severity and even disable them altogether. For an overview of some of the options, check out [this document](https://docs.microsoft.com/en-gb/dotnet/fundamentals/code-analysis/suppress-warnings).
