# SS034 - AccessingTaskResultWithoutAwait

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Use `await` to get the result of an asynchronous operation. While accessing `.Result` is fine once the `Task` has been completed, this removes any ambiguity and helps prevent regressions if the code changes later on.

---

## Violation
```cs
class MyClass
{   
    async Task MyMethod()
    {
        var number = Other().Result;
    }

    async Task<int> Other() => 5;
}
```

## Fix
```cs
class MyClass
{   
    async Task MyMethod()
    {
        var number = await Other();
    }

    async Task<int> Other() => 5;
}
```