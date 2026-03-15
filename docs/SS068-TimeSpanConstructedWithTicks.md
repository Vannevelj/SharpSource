# SS068 - TimeSpanConstructedWithTicks

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

The `TimeSpan` single-parameter constructor accepts ticks (100 nanoseconds), not seconds or milliseconds. `new TimeSpan(30)` creates a duration of 3 microseconds — almost certainly not what the developer intended.

Use the named factory methods such as `TimeSpan.FromSeconds()`, `TimeSpan.FromMilliseconds()`, or `TimeSpan.FromMinutes()` instead, which are unambiguous.

---

## Violation
```cs
var timeout = new TimeSpan(30);
```

## Fix
```cs
var timeout = TimeSpan.FromSeconds(30);
```
