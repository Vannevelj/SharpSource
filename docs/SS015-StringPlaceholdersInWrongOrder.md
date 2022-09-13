# SS015 - StringPlaceholdersInWrongOrder

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Orders the arguments of a `string.Format()` call in ascending order according to index. This reduces the likelihood of the resulting string having data in the wrong place.

---

## Violation
```cs
string s = string.Format("Hello {1}, my name is {0}. Yes you heard that right, {0}.", "Mr. Test", "Mr. Tester");
```

## Fix
```cs
string s = string.Format("Hello {0}, my name is {1}. Yes you heard that right, {1}.", "Mr. Tester", "Mr. Test");
```