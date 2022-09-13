# SS014 - StringDotFormatWithDifferentAmountOfArguments

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A `string.Format()` call lacks arguments and will cause a runtime exception.

---

## Violation
```cs
string s = string.Format(""abc {0}, def {1}"", 1);
```