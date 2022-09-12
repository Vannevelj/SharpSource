# SS007 - FlagsEnumValuesAreNotPowersOfTwo

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

`[Flags]` enum members need to be either powers of two, or bitwise OR expressions. This will fire if they are non-negative decimal literals that are not powers of two, and provide a code fix if the value can be achieved through a binary OR using other enum members.

---

![](./attachments/SS001.gif)