# SS005 - EqualsAndGetHashcodeNotImplementedTogether

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

EqualsAndGetHashcodeNotImplementedTogether  | Implement `Equals()` and `GetHashcode()` together. Implement both to ensure consistent behaviour around lookups.

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