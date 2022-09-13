# SS026 - ExceptionThrownFromEqualityOperator

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from an equality operator. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
public class MyClass
{
    public static bool operator ==(double d, MyClass mc)
    {
        return false;
    }

    public static bool operator !=(double d, MyClass mc)
    {
        throw new ArgumentException();
    }
}
```