# SS038 - HttpContextStoredInField

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

`HttpContext` was stored in a field. This can result in a previous context being used for subsequent requests. Use `IHttpContextAccessor` instead.

---

## Violation
```cs
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext _context;
}
```