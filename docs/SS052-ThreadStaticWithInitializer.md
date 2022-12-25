# SS052 - ThreadStaticWithInitializer

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A field is marked as [ThreadStatic] so it cannot contain an initializer. The field initializer is only executed for the first thread.

---

## Violation
```cs
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random = new Random();
}
```