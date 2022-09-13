# SS034 - AccessingTaskResultWithoutAwait

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Use `await` to get the result of an asynchronous operation. While accessing `.Result` is fine once the `Task` has been completed, this removes any ambiguity and helps prevent regressions if the code changes later on.

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