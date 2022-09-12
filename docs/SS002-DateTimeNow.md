# SS002 - DateTimeNow

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Use `DateTime.UtcNow` to get a locale-independent value. `DateTime.Now` uses the system's local timezone which often means unexpected behaviour when working with global teams/deployments.

---

![](https://user-images.githubusercontent.com/2777107/189771444-02a62f31-76c0-4906-beff-6415c3ebb37e.gif)