# SS055 - MultipleOrderByCalls

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Successive OrderBy() calls will maintain only the last specified sort order. Use ThenBy() to combine them

---

## Violation
```cs
record Data(int X, int Y);
var data = new Data[] { new(1, 3), new(1, 4), new Data(2, 3), new(2, 1), new(1, 1)  };

var ordered = data.OrderBy(obj => obj.X).OrderBy(obj => obj.Y);
```