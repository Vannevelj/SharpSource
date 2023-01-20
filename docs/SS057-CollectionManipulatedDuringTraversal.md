# SS057 - CollectionManipulatedDuringTraversal

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A collection was modified while it was being iterated over. Make a copy first or avoid iterations while the loop is in progress to avoid an `InvalidOperationException` exception at runtime

---

## Violation
```cs
using System.Collections.Generic;

void Method(List<int> items)
{
    foreach (var item in items)
    {
        items.Remove(0);
    }
}
```