# SS039 - EnumWithoutDefaultValue

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An `enum` should specify a default value of 0 as "Unknown" or "None". When an invalid enum value is marshalled or you receive a default value, many systems return it as `0`. This way you don't inadvertedly interpret it as a valid value.

---

## Violation
```cs
enum Test 
{
    A
}
```