# SS032 - ThreadSleepInAsyncMethod

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Synchronously sleeping a thread in an `async` method combines two threading models and can lead to deadlocks.

---

## Violation
```cs
async Task MyMethod()
{
    Thread.Sleep(5000);
}
```

## Fix
```cs
async Task MyMethod()
{
    await Task.Delay(5000);
}
```