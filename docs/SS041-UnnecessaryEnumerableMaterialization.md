# SS041 - UnnecessaryEnumerableMaterialization

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An `IEnumerable` was materialized before a deferred execution call. This generally results in unnecessary work being done.

---

## Violation
```cs
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { "test" };
values.ToList().Take(1);
```

## Fix
```cs
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { "test" };
values.Take(1);
```