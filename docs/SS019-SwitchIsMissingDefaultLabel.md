# SS019 - SwitchIsMissingDefaultLabel

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Switch is missing a `default` label. Include this to provide fallback behaviour for any missing cases, including when the upstream API adds them later on.

---

## Violation
```cs
var e = MyEnum.Fizz;
switch (e)
{
    case MyEnum.Fizz:
    case MyEnum.Buzz:
        break;
}

enum MyEnum
{
    Fizz, Buzz, FizzBuzz
}
```

## Fix
```cs
var e = MyEnum.Fizz;
switch (e)
{
    case MyEnum.Fizz:
    case MyEnum.Buzz:
        break;
    default:
        throw new ArgumentException("Unsupported value");
}

enum MyEnum
{
    Fizz, Buzz, FizzBuzz
}
```