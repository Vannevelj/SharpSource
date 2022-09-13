# SS029 - ExceptionThrownFromGetHashCode

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a `GetHashCode()` method. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
public class MyClass
{
    public override int GetHashCode()
    {
        throw new ArgumentException();
    }
}
```