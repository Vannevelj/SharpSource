# SS013 - RethrowExceptionWithoutLosingStacktrace

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An exception is rethrown in a way that it loses the stacktrace. Use an empty `throw;` statement instead to preserve it.

---

![](./attachments/SS001.gif)