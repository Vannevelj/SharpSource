# SS002 - DateTimeNow

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Use `DateTime.UtcNow` to get a locale-independent value. `DateTime.Now` uses the system's local timezone which often means unexpected behaviour when working with global teams/deployments.

---

![](./attachments/SS002.gif)