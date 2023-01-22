# SS005 - EqualsAndGetHashcodeNotImplementedTogether

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Implement `Equals()` and `GetHashcode()` together. Implement both to ensure consistent behaviour around lookups.

---

## Violation
```cs
class MyClass
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }
}
```

## Fix
```cs
class MyClass
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}
```