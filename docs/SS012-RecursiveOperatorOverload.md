# SS012 - RecursiveOperatorOverload

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

Recursively using overloaded operator will result in a stack overflow when attempting to use it.

---

## Violation
```cs
async void WriteFile()
{
    await File.WriteAllTextAsync("c:/temp", "content")
}
```

## Fix
```cs
async Task WriteFile()
{
    await File.WriteAllTextAsync("c:/temp", "content")
}
```