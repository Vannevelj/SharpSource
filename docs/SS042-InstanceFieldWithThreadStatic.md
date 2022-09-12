# SS042 - InstanceFieldWithThreadStatic

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

`[ThreadStatic]` can only be used on static fields. If used on an instance field the attribute will not have any effect and the subsequent multithreading behaviour will not be as intended.

---

![](./attachments/SS001.gif)