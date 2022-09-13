# SS008 - GetHashCodeRefersToMutableMember

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

`GetHashCode(`) refers to mutable or static member. If the object is used in a collection and then is mutated, subsequent lookups will result in a different hash and might cause lookups to fail.

---

## Violation
```cs
public class Foo
{
    private char _boo = '1';

    public override int GetHashCode()
    {
        return _boo.GetHashCode();
    }
}
```