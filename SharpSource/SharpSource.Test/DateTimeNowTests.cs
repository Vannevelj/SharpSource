using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.DateTimeNowAnalyzer, SharpSource.Diagnostics.DateTimeNowCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class DateTimeNowTests
{
    [TestMethod]
    public async Task DateTimeNow_UtcNow()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var date = DateTime.UtcNow;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DateTimeNow_Now_Expression()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Console.WriteLine({|#0:DateTime.Now|});
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Console.WriteLine(DateTime.UtcNow);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use DateTime.UtcNow to get a locale-independent value"), result);
    }

    [TestMethod]
    public async Task DateTimeNow_Now_Expression_UsingStatic()
    {
        var original = @"
using static System.DateTime;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            System.Console.WriteLine({|#0:Now|});
        }
    }
}";

        var result = @"
using static System.DateTime;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            System.Console.WriteLine(UtcNow);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use DateTime.UtcNow to get a locale-independent value"), result);
    }

    [TestMethod]
    public async Task DateTimeNow_FullName()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var date = {|#0:System.DateTime.Now|};
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var date = System.DateTime.UtcNow;
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use DateTime.UtcNow to get a locale-independent value"), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/179")]
    public async Task DateTimeNow_TopLevelStatements()
    {

        var original = @"
using System;
var date = {|#0:DateTime.Now|};";

        var result = @"
using System;
var date = DateTime.UtcNow;";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use DateTime.UtcNow to get a locale-independent value"), result);
    }

    [TestMethod]
    public async Task DateTimeNow_Nameof()
    {

        var original = @"
using System;
var date = nameof(DateTime.Now);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}