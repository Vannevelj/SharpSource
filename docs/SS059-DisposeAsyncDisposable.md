# SS059 - DisposeAsyncDisposable

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An object implements `IAsyncDisposable` and can be disposed of asynchronously in the context it is used.

---

## Violation
```cs
using System.IO;
using System.Threading.Tasks;

async Task Method()
{
    using var stream = new FileStream("", FileMode.Create);
}
```

## Fix
```cs
using System.IO;
using System.Threading.Tasks;

async Task Method()
{
    await using var stream = new FileStream("", FileMode.Create);
}
```