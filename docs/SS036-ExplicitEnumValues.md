# SS036 - ExplicitEnumValues

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An enum should explicitly specify its values. Otherwise you risk serializing your enums into different numeric values if you add a new member at any place other than the last line in the enum file.

---

![](./attachments/SS001.gif)