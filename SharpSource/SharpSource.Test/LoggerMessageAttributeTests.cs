using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LoggerMessageAttributeAnalyzer, SharpSource.Diagnostics.LoggerMessageAttributeCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class LoggerMessageAttributeTests
{
    [TestMethod]
    public async Task LoggerMessageAttribute_LogInformation_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation(""User logged in"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogUserLoggedIn();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User logged in"")]
        private partial void LogUserLoggedIn();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_LogWarning_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogWarning(""Cache miss detected"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogCacheMissDetected();
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = ""Cache miss detected"")]
        private partial void LogCacheMissDetected();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogWarning"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_LogError_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogError(""Failed to process request"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogFailedToProcessRequest();
        }

        [LoggerMessage(Level = LogLevel.Error, Message = ""Failed to process request"")]
        private partial void LogFailedToProcessRequest();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogError"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_LogDebug_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogDebug(""Entering method"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogEnteringMethod();
        }

        [LoggerMessage(Level = LogLevel.Debug, Message = ""Entering method"")]
        private partial void LogEnteringMethod();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogDebug"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_LogCritical_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogCritical(""System failure"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogSystemFailure();
        }

        [LoggerMessage(Level = LogLevel.Critical, Message = ""System failure"")]
        private partial void LogSystemFailure();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogCritical"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_LogTrace_WithMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogTrace(""Verbose trace"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogVerboseTrace();
        }

        [LoggerMessage(Level = LogLevel.Trace, Message = ""Verbose trace"")]
        private partial void LogVerboseTrace();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogTrace"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_Log_WithLogLevel()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.Log(LogLevel.Warning, ""Something happened"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogSomethingHappened();
        }

        [LoggerMessage(Level = LogLevel.Warning, Message = ""Something happened"")]
        private partial void LogSomethingHappened();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of Log"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_Log_FullyQualifiedStaticCall()
    {
        // Extension method called as static method without using statement
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public MyClass(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:Microsoft.Extensions.Logging.LoggerExtensions.Log(_logger, Microsoft.Extensions.Logging.LogLevel.Error, ""An error occurred"")|};
        }
    }
}";

        // Just verify the diagnostic is raised (code fix for static call syntax is complex)
        await VerifyCS.VerifyAnalyzerAsync(original, null,
            VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of Log"));
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_WithPlaceholders()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string userId, int count)
        {
            {|#0:_logger.LogInformation(""User {UserId} performed {Count} actions"", userId, count)|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string userId, int count)
        {
            LogUserUseridPerformedCount(userId, count);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User {UserId} performed {Count} actions"")]
        private partial void LogUserUseridPerformedCount(object userId, object count);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_ClassAlreadyPartial()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation(""User logged in"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogUserLoggedIn();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User logged in"")]
        private partial void LogUserLoggedIn();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_UnrelatedMethod_NoDiagnostic()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            _logger.IsEnabled(LogLevel.Information);
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_DifferentLoggerInterface_NoDiagnostic()
    {
        // Use a completely different ILogger interface that's not from Microsoft.Extensions.Logging
        var original = @"
namespace ConsoleApplication1
{
    interface ILogger
    {
        void LogInformation(string message);
    }

    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            _logger.LogInformation(""User logged in"");
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_CustomLogMethod_NoDiagnostic()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogSomething(""test"");
        }

        void LogSomething(string message)
        {
            // Not an ILogger method
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_TopLevelStatement_NoDiagnostic()
    {
        // Top-level statements don't have a containing type to add the partial method to
        var original = @"
using Microsoft.Extensions.Logging;

ILogger logger = null!;
logger.LogInformation(""Application started"");
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_FullyQualifiedLoggerType()
    {
        // Uses fully qualified type names but still needs using for extension methods
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public MyClass(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation(""User logged in"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public MyClass(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogUserLoggedIn();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User logged in"")]
        private partial void LogUserLoggedIn();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_CustomLoggerInterfaceInheritingFromILogger()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    interface IMyLogger : ILogger
    {
        void LogCustom(string message);
    }

    class MyClass
    {
        private readonly IMyLogger _logger;

        public MyClass(IMyLogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation(""User logged in"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    interface IMyLogger : ILogger
    {
        void LogCustom(string message);
    }

    partial class MyClass
    {
        private readonly IMyLogger _logger;

        public MyClass(IMyLogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogUserLoggedIn();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User logged in"")]
        private partial void LogUserLoggedIn();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_NonConstantMessage()
    {
        // When the message is not a constant, we still flag it and generate a message template from parameters
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string message)
        {
            {|#0:_logger.LogInformation(message)|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string message)
        {
            LogInformation(message);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""{arg0}"")]
        private partial void LogInformation(object arg0);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_MultiplePlaceholders()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string userId, string action, int duration, bool success)
        {
            {|#0:_logger.LogInformation(""User {UserId} performed {Action} in {Duration}ms with success={Success}"", userId, action, duration, success)|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod(string userId, string action, int duration, bool success)
        {
            LogUserUseridPerformedAction(userId, action, duration, success);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""User {UserId} performed {Action} in {Duration}ms with success={Success}"")]
        private partial void LogUserUseridPerformedAction(object userId, object action, object duration, object success);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_MultiplePlaceholders_NonLiteralArguments()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            var user = GetUser();
            var count = GetCount();
            {|#0:_logger.LogInformation(""User {UserId} performed {Count} actions"", user.Id, count + 1)|};
        }

        User GetUser() => new User();
        int GetCount() => 5;
    }

    class User { public string Id { get; set; } }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            var user = GetUser();
            var count = GetCount();
            LogUserUseridPerformedCount(user.Id, count + 1);
        }

        User GetUser() => new User();
        int GetCount() => 5;
        [LoggerMessage(Level = LogLevel.Information, Message = ""User {UserId} performed {Count} actions"")]
        private partial void LogUserUseridPerformedCount(object userId, object count);
    }

    class User { public string Id { get; set; } }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_SingleCharacterWords()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation(""A B C"")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogABC();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = ""A B C"")]
        private partial void LogABC();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }

    [TestMethod]
    public async Task LoggerMessageAttribute_EmptyMessage()
    {
        var original = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            {|#0:_logger.LogInformation("""")|};
        }
    }
}";

        var result = @"
using Microsoft.Extensions.Logging;

namespace ConsoleApplication1
{
    partial class MyClass
    {
        private readonly ILogger _logger;

        public MyClass(ILogger logger)
        {
            _logger = logger;
        }

        void MyMethod()
        {
            LogInformation();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = """")]
        private partial void LogInformation();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use the LoggerMessage attribute for high-performance logging instead of LogInformation"), result, disabledDiagnostics: ["CS8795"]);
    }
}
