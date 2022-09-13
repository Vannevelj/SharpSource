# SS035 - SynchronousTaskWait

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Asynchronously `await` tasks instead of blocking them to avoid deadlocks.

---

## Violation
```cs
async Task MyMethod()
{
    Task.Delay(1).Wait();
}
```

## Fix
```cs
async Task MyMethod()
{
    await Task.Delay(1);
}
```