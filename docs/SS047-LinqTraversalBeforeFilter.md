# SS047 - LinqTraversalBeforeFilter

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

An `IEnumerable` extension method was used to traverse the collection and is subsequently filtered using `Where()`. If the `Where()` filter is executed first, the traversal will have to iterate over fewer items which will result in better performance.

---

![](./attachments/SS001.gif)