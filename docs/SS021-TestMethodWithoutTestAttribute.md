# SS021 - TestMethodWithoutTestAttribute

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A method might be missing a test attribute. Helps ensure no unit tests are missing from your test runs.

---

## Violation
```cs
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
class MyClass
{
    public void MyMethod()
    {
    }
}
```