# SS024 - ExceptionThrownFromStaticConstructor

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a `static` constructor. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
public class MyClass
{
    static MyClass()
    {
        throw new ArgumentException();
    }
}
```