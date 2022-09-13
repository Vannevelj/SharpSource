# SS040 - UnusedResultOnImmutableObject

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

The result of an operation on a `string` is unused. At best this has no effect, at worst this means a desired `string` operation has not been performed.

---

## Violation
```cs
void Method() 
{
    "test ".Trim();
}
```