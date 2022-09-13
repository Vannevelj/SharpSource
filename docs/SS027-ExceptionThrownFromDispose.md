# SS027 - ExceptionThrownFromDispose

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a `Dispose()` method. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
public class MyClass : IDisposable
{
    public void Dispose()
    {
        throw new ArgumentException();
    }
}
```