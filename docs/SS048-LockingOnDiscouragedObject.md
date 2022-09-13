# SS048 - LockingOnDiscouragedObject

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A `lock` was taken using an instance of a discouraged type. `System.String`, `System.Type` and `this` references can all lead to deadlocks and should be replaced with a `System.Object` instance instead.

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