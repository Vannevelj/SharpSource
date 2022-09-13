# SS025 - ExceptionThrownFromFinallyBlock

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An exception is thrown from a `finally` block. This does not trigger for `NotImplementedException` and `NotSupportedException`.

---

## Violation
```cs
try 
{ 

} 
finally 
{ 
    throw new ArgumentException(); 
}
```