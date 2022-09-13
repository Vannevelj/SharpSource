# SS028 - ExceptionThrownFromFinalizer

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a finalizer method

---

## Violation
```cs
public class MyClass
{
    ~MyClass()
    {
        throw new ArgumentException();
    }
}
```