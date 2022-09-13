# SS003 - DivideIntegerByInteger

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

The operands of a divisive expression are both integers and result in an implicit rounding.

---

## Violation
```cs
int result = 5 / 6;
```