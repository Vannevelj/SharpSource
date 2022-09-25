# SS050 - ParameterAssignedInConstructor

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

A parameter was assigned in a constructor

---

## Violation
```cs
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        count = Count;
    }
}
```

## Fix
```cs
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        Count = count;
    }
}
```