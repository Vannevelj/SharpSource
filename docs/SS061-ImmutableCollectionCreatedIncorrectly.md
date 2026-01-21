# SS061 - ImmutableCollectionCreatedIncorrectly

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

`ImmutableArray<T>` is being created using the `new` keyword instead of the `ImmutableArray.Create<T>()` method. Using `new ImmutableArray<T>()` creates an uninitialized struct which can lead to runtime exceptions when attempting to use the collection.

---

## Violation

```csharp
var array = new ImmutableArray<int>();
```

## Fix

```csharp
var array = ImmutableArray.Create<int>();
```

---

## Remarks
The `ImmutableArray<T>` type is a struct, and creating it with `new` results in an uninitialized (default) instance. This uninitialized instance will throw a `NullReferenceException` when you try to access its members or enumerate it. Always use `ImmutableArray.Create<T>()` or other factory methods to ensure the collection is properly initialized.

For more information, see the [official documentation](https://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable.immutablearray.create).