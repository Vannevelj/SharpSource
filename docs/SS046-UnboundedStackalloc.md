# SS046 - UnboundedStackalloc

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An array is stack allocated without checking whether the length is within reasonable bounds. This can result in performance degradations and security risks.

---

## Violation
```cs
var len = new Random().Next();
Span<int> values = stackalloc int[len];";
```

## Fix
```cs
var len = new Random().Next();
Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];
```