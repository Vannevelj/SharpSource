# SS007 - FlagsEnumValuesAreNotPowersOfTwo

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

`[Flags]` enum members need to be either powers of two, or bitwise OR expressions. This will fire if they are non-negative decimal literals that are not powers of two, and provide a code fix if the value can be achieved through a binary OR using other enum members.

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