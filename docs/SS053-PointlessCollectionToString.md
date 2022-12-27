# SS053 - PointlessCollectionToString

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

`.ToString()` was called on a collection which results in impractical output. Considering using `string.Join()` to display the values instead.

---

## Violation
```cs
using System;
using System.Collections.Generic;

var collection = new List<int>();
Console.Write(collection.ToString());
```