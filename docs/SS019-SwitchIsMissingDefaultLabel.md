# SS019 - SwitchIsMissingDefaultLabel

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Async methods should return a `Task` to make them awaitable. Without it, execution continues before the asynchronous `Task` has finished and exceptions go unhandled.

---

![](./attachments/SS001.gif)