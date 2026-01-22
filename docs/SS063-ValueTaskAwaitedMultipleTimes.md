# SS063 - ValueTaskAwaitedMultipleTimes

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A `ValueTask` or `ValueTask<T>` was awaited multiple times. Unlike `Task`, a `ValueTask` can only be consumed once. Awaiting it more than once can lead to undefined behavior, including exceptions or incorrect results.

According to Microsoft's documentation:
> A ValueTask instance may only be awaited once, and consumers may not call GetAwaiter() until the instance has completed.

---

## Violation
```cs
async Task Method()
{
    ValueTask GetValueTask() => ValueTask.CompletedTask;
    
    var task = GetValueTask();
    await task;
    await task; // Second await on the same ValueTask
}
```

## See also
- [ValueTask Struct (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1?view=net-8.0#remarks)
