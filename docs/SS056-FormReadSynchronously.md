# SS056 - FormReadSynchronously

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

Synchronously accessed `HttpRequest.Form` which uses sync-over-async. Use `HttpRequest.ReadFormAsync()` instead.

---

## Violation
```cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

class MyController : Controller
{
    public IActionResult Post()
    {
        var form = HttpContext.Request.Form;
        return Ok();
    }
}
```