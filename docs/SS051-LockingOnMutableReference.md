# SS051 - LockingOnMutableReference

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A lock was obtained on a mutable field which can lead to deadlocks when a new value is assigned. Mark the field as `readonly` to prevent re-assignment after a lock is taken.

---

## Violation
```cs
class Test
{
    private object _lock = new object();
}
```

## Fix
```cs
class Test
{
    private readonly object _lock = new object();
}
```