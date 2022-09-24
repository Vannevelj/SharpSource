# SS049 - ComparingStringsWithoutStringComparison

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A `string` is being compared through allocating a new `string`, e.g. using `ToLower()` or `ToUpperInvariant()`. Use a case-insensitive comparison instead which does not allocate.

---

## Violation
```cs
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.ToLower() == s2.ToLower();
```

## Fix
```cs
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
```