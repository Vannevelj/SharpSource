using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class SynchronousTaskWaitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SynchronousTaskWaitAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new SynchronousTaskWaitCodeFix();

    [TestMethod]
    public async Task SynchronousTaskWait_AsyncContext()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            Task.Delay(1).Wait();
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Task.Delay(1);
        }
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_SyncContext()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        void MyMethod()
        {
            Task.Delay(1).Wait();
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_AsyncContext_Void()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            Task.Delay(1).Wait();
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            await Task.Delay(1);
        }
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_ExpressionBodiedMember()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod() => Task.Delay(1).Wait();
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod() => await Task.Delay(1);
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_AsyncLambda()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = async () => Task.Delay(1).Wait();
	lambda();
}
";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = async () => await Task.Delay(1);
	lambda();
}
";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [Ignore("See https://github.com/Vannevelj/SharpSource/issues/268")]
    public async Task SynchronousTaskWait_AsyncLambda_AsAnonymousFunction()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(async () => Task.Delay(1).Wait());
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(async () => await Task.Delay(1));
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_SyncLambda()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(() => Task.Delay(1).Wait());
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_Constructor()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        MyClass()
        {
            Other().Wait();
        }

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_ChainedExpression()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            Get.Wait();
        }

        Task Get => Task.CompletedTask;
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Get;
        }

        Task Get => Task.CompletedTask;
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/263")]
    public async Task SynchronousTaskWait_PreservesLeadingTrivia()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class Test
{
    async Task MyMethod()
    {
        // Wait a bit
        Task.Delay(1).Wait();
    }
}";

        var result = @"
using System;
using System.Threading.Tasks;

class Test
{
    async Task MyMethod()
    {
        // Wait a bit
        await Task.Delay(1);
    }
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_TopLevelMethod()
    {
        var original = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    Task.Delay(1).Wait();
}";

        var result = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    await Task.Delay(1);
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_TopLevelStatement()
    {
        var original = @"
using System;
using System.Threading.Tasks;

Task.Delay(1).Wait();";

        var result = @"
using System;
using System.Threading.Tasks;

await Task.Delay(1);";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/245")]
    public async Task SynchronousTaskWait_WhenTimeoutIsPassed()
    {
        var original = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    Task.Delay(1).Wait(10000);
}";

        var result = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    Task.Delay(1).Wait(10000);
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_LocalFunction()
    {
        var original = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    async Task InnerMethod() => Task.Delay(1).Wait();

    await InnerMethod();
}";

        var result = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    async Task InnerMethod() => await Task.Delay(1);

    await InnerMethod();
}";

        await VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
        await VerifyFix(original, result);
    }
}