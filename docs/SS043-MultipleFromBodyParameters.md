# SS043 - MultipleFromBodyParameters

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A method specifies multiple `[FromBody]` parameters but only one is allowed. Specify a wrapper type or use `[FromForm]`, `[FromRoute]`, `[FromHeader]` and `[FromQuery]` instead.

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