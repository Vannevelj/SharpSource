# SS017 - StructWithoutElementaryMethodsOverridden

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Structs should implement `Equals()`, `GetHashCode()`, and `ToString()`. By default they use reflection which comes with performance penalties.

---

![](./attachments/SS017.gif)