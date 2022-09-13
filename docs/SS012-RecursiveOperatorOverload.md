# SS012 - RecursiveOperatorOverload

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

Recursively using overloaded operator will result in a stack overflow when attempting to use it.

---

## Violation
```cs
public class A
{
    public static A operator ==(A a1, A a2)
    {
        return a1 == a2;
    }

    public static A operator !=(A a1, A a2)
    {
        return a1 != a2;
    }
}
```