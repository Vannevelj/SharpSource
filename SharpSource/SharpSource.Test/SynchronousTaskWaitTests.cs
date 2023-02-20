using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.SynchronousTaskWaitAnalyzer, SharpSource.Diagnostics.SynchronousTaskWaitCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class SynchronousTaskWaitTests
{
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
            {|#0:Task.Delay(1).Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
            {|#0:Task.Delay(1).Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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
        async Task MyMethod() => {|#0:Task.Delay(1).Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_AsyncLambda()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = async () => {|#0:Task.Delay(1).Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
    }

    [TestMethod]
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
	    Action MyMethod() => new Action(async () => {|#0:Task.Delay(1).Wait()|});
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
            {|#0:Get.Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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
        {|#0:Task.Delay(1).Wait()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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
    {|#0:Task.Delay(1).Wait()|};
}";

        var result = @"
using System;
using System.Threading.Tasks;

await MyMethod();

async Task MyMethod()
{
    await Task.Delay(1);
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
    }

    [TestMethod]
    public async Task SynchronousTaskWait_TopLevelStatement()
    {
        var original = @"
using System;
using System.Threading.Tasks;

{|#0:Task.Delay(1).Wait()|};";

        var result = @"
using System;
using System.Threading.Tasks;

await Task.Delay(1);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
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
    {|#0:Task.Delay(1).Wait(10000)|};
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"));
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
    async Task InnerMethod() => {|#0:Task.Delay(1).Wait()|};

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Asynchronously wait for task completion using await instead"), result);
    }
}