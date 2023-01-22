# SS036 - ExplicitEnumValues

[![Generic badge](https://img.shields.io/badge/Severity-Info-blue.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An enum should explicitly specify its values. Otherwise you risk serializing your enums into different numeric values if you add a new member at any place other than the last line in the enum file.

---

## Violation
```cs
enum Test 
{
    A
}
```

## Fix
```cs
enum Test 
{
    A = 0
}
```