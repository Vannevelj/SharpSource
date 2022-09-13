# SS010 - NewGuid

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An empty GUID was created in an ambiguous manner. The default `Guid` constructor creates an instance with an empty value which is rarely what you want.

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