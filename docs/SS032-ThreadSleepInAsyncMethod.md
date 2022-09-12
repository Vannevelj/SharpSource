# SS032 - ThreadSleepInAsyncMethod

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Synchronously sleeping a thread in an `async` method combines two threading models and can lead to deadlocks.

---

![](./attachments/SS032.gif)