# SS046 - UnboundedStackalloc

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An array is stack allocated without checking whether the length is within reasonable bounds. This can result in performance degradations and security risks.

---

![](./attachments/SS001.gif)