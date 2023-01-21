# SS058 - StringConcatenatedInLoop

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A `string` was concatenated in a loop which introduces intermediate allocations. Consider using a `StringBuilder` or pre-allocated `string` instead.

---

## Violation
```cs
using System.Collections.Generic;

void Method(List<int> items)
{
    var result = "";
    foreach (var item in items)
    {
        result += item;
    }
}
```