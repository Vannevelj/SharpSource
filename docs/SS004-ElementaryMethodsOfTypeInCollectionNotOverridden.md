# SS004 - ElementaryMethodsOfTypeInCollectionNotOverridden

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

ElementaryMethodsOfTypeInCollectionNotOverridden  | Implement `Equals()` and `GetHashcode()` methods for a type used in a collection. Collections use these to fetch objects but by default they use reference equality. Depending on where your objects come from, they might be missed in the lookup.

---

![](./attachments/SS004.gif)