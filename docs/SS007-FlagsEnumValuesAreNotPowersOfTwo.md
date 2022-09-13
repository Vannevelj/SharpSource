# SS007 - FlagsEnumValuesAreNotPowersOfTwo

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

`[Flags]` enum members need to be either powers of two, or bitwise OR expressions. This will fire if they are non-negative decimal literals that are not powers of two, and provide a code fix if the value can be achieved through a binary OR using other enum members.

---

## Violation
```cs
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 3,
    Boz = 4
}
```

## Fix
```cs
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = Biz | Baz,
    Boz = 4
}
```