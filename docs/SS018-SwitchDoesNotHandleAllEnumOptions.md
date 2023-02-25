# SS018 - SwitchDoesNotHandleAllEnumOptions

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Add cases for missing enum member. That way you won't miss new behaviour in the consuming API since it will be explicitly handled. If a `default` clause is present, this becomes an INFO diagnostic instead.

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
    case MyEnum.FizzBuzz:
        throw new System.NotImplementedException();
    case MyEnum.Fizz:
    case MyEnum.Buzz:
        break;
}

enum MyEnum
{
    Fizz, Buzz, FizzBuzz
}
```