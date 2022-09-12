# SS009 - LoopedRandomInstantiation

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An instance of type `System.Random` is created in a loop. `Random` uses a time-based seed so when used in a fast loop it will end up with multiple identical seeds for subsequent invocations.

---

![](./attachments/SS001.gif)