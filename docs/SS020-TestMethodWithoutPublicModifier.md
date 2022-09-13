# SS020 - TestMethodWithoutPublicModifier

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Verifies whether a test method has the `public` modifier. Some test frameworks require this to discover unit tests. This works for xUnit, NUnit and MSTest.

---

## Violation
```cs
using NUnit.Framework;

[TestFixture]
public class MyClass
{
    [Test]
    internal void Method()
    {

    }
}
```

## Fix
```cs
using NUnit.Framework;

[TestFixture]
public class MyClass
{
    [Test]
    public void Method()
    {

    }
}
```