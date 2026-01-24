# SS065 - LoggerMessageAttribute

[![Generic badge](https://img.shields.io/badge/Severity-Info-blue.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

The `LoggerMessage` attribute provides a high-performance logging API that eliminates boxing, temporary allocations, and reduces overhead compared to standard `ILogger` extension methods. This analyzer suggests converting regular logging calls to the source-generated logging pattern.

For more information, see [Compile-time logging source generation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator).

---

## Violation
```cs
public class MyService
{
    private readonly ILogger _logger;

    public MyService(ILogger logger)
    {
        _logger = logger;
    }

    public void DoWork(string userId)
    {
        _logger.LogInformation("User {UserId} started work", userId);
    }
}
```

## Fix
```cs
public partial class MyService
{
    private readonly ILogger _logger;

    public MyService(ILogger logger)
    {
        _logger = logger;
    }

    public void DoWork(string userId)
    {
        LogUserStartedWork(userId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} started work")]
    private partial void LogUserStartedWork(string userId);
}
```

## Benefits

- **Performance**: Source-generated logging avoids boxing of value types and reduces allocations
- **Structure**: Log messages are parsed at compile time rather than runtime
- **Consistency**: Enforces a consistent logging pattern across the codebase
- **Type Safety**: Parameters are strongly typed in the generated method

## Notes

This analyzer only triggers when `Microsoft.Extensions.Logging.LoggerMessageAttribute` is available in the project (typically .NET 6+).
