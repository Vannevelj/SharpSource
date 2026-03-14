# SS066 - DisposableFieldIsNotDisposed

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A type owns a field that implements `IDisposable` or `IAsyncDisposable`, but that field is not included in any reachable disposal path. This can leave resources undisposed and eventually lead to leaks.

Indirect disposal paths are taken into account as well, such as helper methods, property indirection, default interface implementations and compiler-generated backing fields for properties.

---

## Violation
```cs
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
    }
}
```

## Fix
```cs
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
        _stream.Dispose();
    }
}
```