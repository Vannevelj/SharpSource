# SS042 - InstanceFieldWithThreadStatic

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

`[ThreadStatic]` can only be used on static fields. If used on an instance field the attribute will not have any effect and the subsequent multithreading behaviour will not be as intended.

---

## Violation
```cs
class MyClass
{
    [ThreadStatic]
    int _field;
}
```