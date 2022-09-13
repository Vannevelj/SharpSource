# SS022 - ExceptionThrownFromImplicitOperator

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from an `implicit` operator
---

## Violation
```cs
public class MyClass
{
    public static implicit operator MyClass(double d)
    {
        throw new ArgumentException();
    }
}
```