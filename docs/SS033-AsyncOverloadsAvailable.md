# SS033 - AsyncOverloadsAvailable

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An `async` overload is available. These overloads typically exist to provide better performing IO calls and should generally be preferred.

---

## Violation
```cs
using System.IO;

async void MyMethod()
{
    new StringWriter().Write("");
}
```

## Fix
```cs
using System.IO;

async void MyMethod()
{
    await new StringWriter().WriteAsync("");
}
```