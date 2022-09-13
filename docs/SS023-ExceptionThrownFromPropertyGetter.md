# SS023 - ExceptionThrownFromPropertyGetter

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a property getter. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
public class MyClass
{
    public int MyProp
    {
        get
        {
            throw new ArgumentException();
        }
        set
        {

        }
    }
}
```