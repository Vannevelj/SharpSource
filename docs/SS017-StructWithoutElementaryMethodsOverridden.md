# SS017 - StructWithoutElementaryMethodsOverridden

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Structs should implement `Equals()`, `GetHashCode()`, and `ToString()`. By default they use reflection which comes with performance penalties.

---

## Violation
```cs
struct X
{
}
```

## Fix
```cs
struct X
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}
```