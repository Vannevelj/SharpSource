# SS048 - LockingOnDiscouragedObject

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A `lock` was taken using an instance of a discouraged type. `System.String`, `System.Type` and `this` references can all lead to deadlocks and should be replaced with a `System.Object` instance instead.

---

## Violation
```cs
class Test
{
    private Type _badLock1 = default;
    private string _badLock2 = default;

    void Method()
    {
        lock (_badLock1) { }
        lock (_badLock2) { }
        lock (this) { }
    }
}
```