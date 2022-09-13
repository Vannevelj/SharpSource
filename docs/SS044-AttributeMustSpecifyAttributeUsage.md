# SS044 - AttributeMustSpecifyAttributeUsage

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An attribute was defined without specifying the `[AttributeUsage]`.

---

## Violation
```cs
class MyAttribute : Attribute
{
}
```

## Fix
```cs
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}
```