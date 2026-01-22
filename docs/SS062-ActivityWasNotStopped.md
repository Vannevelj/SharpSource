# SS062 - ActivityWasNotStopped

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An `Activity` was started using `ActivitySource.StartActivity()` but is not being stopped or disposed. Activities should either be stopped explicitly with `Stop()`, disposed with `Dispose()`, or wrapped in a `using` statement to ensure they are properly ended.

---

## Violation
```cs
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new("Test");

    void Method()
    {
        var activity = Source.StartActivity("test");
        // Activity is never stopped
    }
}
```

## Fix (using statement)
```cs
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new("Test");

    void Method()
    {
        using var activity = Source.StartActivity("test");
        // Activity is automatically disposed at end of scope
    }
}
```

## Fix (explicit Stop)
```cs
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new("Test");

    void Method()
    {
        var activity = Source.StartActivity("test");
        try
        {
            // Do work
        }
        finally
        {
            activity?.Stop();
        }
    }
}
```
