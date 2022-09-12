# SS001 - AsyncMethodWithVoidReturnType

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Async methods should return a `Task` to make them awaitable. Without it, execution continues before the asynchronous `Task` has finished and exceptions go unhandled.

---

![](https://user-images.githubusercontent.com/2777107/189770723-226e60e0-2e26-47f1-a4b5-b0a8cfbf753f.gif)