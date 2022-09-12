# SS045 - StaticInitializerAccessedBeforeInitialization

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A `static` field relies on the value of another `static` field which is defined in the same type. `static` fields are initialized in order of appearance.

---

![](./attachments/SS001.gif)