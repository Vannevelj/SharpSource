# SS064 - UnnecessaryToStringOnSpan

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Calling `ToString()` on a `Span<char>` or `ReadOnlySpan<char>` is unnecessary when an overload that directly accepts spans is available. Using the span-accepting overload avoids allocating a new string and improves performance.

---

## Violation
```cs
ReadOnlySpan<char> span = "hello world".AsSpan();
Console.WriteLine(span.ToString());
```

## Fix
```cs
ReadOnlySpan<char> span = "hello world".AsSpan();
Console.WriteLine(span);
```

## Why?

Many APIs in .NET now have overloads that accept `Span<char>` or `ReadOnlySpan<char>` directly. When you call `ToString()` on a span just to pass it to such a method, you:

1. **Allocate a new string**: The `ToString()` call creates a new string object on the heap
2. **Copy the data**: The characters are copied from the span to the new string
3. **Lose the benefit of spans**: The whole point of spans is to avoid allocations

By using the span-accepting overload directly, you avoid these costs entirely.

## Common scenarios

- `Console.WriteLine(span)` instead of `Console.WriteLine(span.ToString())`
- `StringBuilder.Append(span)` instead of `StringBuilder.Append(span.ToString())`
- `TextWriter.Write(span)` instead of `TextWriter.Write(span.ToString())`
