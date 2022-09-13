using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class DateTimeNowTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new DateTimeNowAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new DateTimeNowCodeFix();


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

        await VerifyDiagnostic(original);
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
            Console.WriteLine(DateTime.Now);
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

        await VerifyDiagnostic(original, "Use DateTime.UtcNow to get a locale-independent value");
        await VerifyFix(original, result);
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
            var date = System.DateTime.Now;
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

        await VerifyDiagnostic(original, "Use DateTime.UtcNow to get a locale-independent value");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/179")]
    public async Task DateTimeNow_TopLevelStatements()
    {

        var original = @"
using System;
var date = DateTime.Now;";

        var result = @"
using System;
var date = DateTime.UtcNow;";

        await VerifyDiagnostic(original, "Use DateTime.UtcNow to get a locale-independent value");
        await VerifyFix(original, result);
    }
}