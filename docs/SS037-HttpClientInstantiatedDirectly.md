# SS037 - HttpClientInstantiatedDirectly

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

`HttpClient` was instantiated directly. This can result in socket exhaustion and DNS issues in long-running scenarios. Use `IHttpClientFactory` instead.

---

![](./attachments/SS001.gif)