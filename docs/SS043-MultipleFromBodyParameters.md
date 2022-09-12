# SS043 - MultipleFromBodyParameters

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A method specifies multiple `[FromBody]` parameters but only one is allowed. Specify a wrapper type or use `[FromForm]`, `[FromRoute]`, `[FromHeader]` and `[FromQuery]` instead.

---

![](./attachments/SS001.gif)