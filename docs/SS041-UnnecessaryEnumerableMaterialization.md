# SS041 - UnnecessaryEnumerableMaterialization

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An `IEnumerable` was materialized before a deferred execution call. This generally results in unnecessary work being done.

---

![](./attachments/SS001.gif)