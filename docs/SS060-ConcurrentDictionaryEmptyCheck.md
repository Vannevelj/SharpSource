# SS060 - ConcurrentDictionaryEmptyCheck

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A `ConcurrentDictionary` is checked for emptiness without using `.IsEmpty`. Alternatives such as `.Count == 0`, `.Any()` or `.Count()` will take a global lock on the entire dictionary which can have negative performance consequences.

---

## Violation
```cs
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.Count == 0;
```

## Fix
```cs
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.IsEmpty;
```