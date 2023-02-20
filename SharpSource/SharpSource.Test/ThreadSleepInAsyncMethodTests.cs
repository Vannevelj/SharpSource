using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ThreadSleepInAsyncMethodAnalyzer, SharpSource.Diagnostics.ThreadSleepInAsyncMethodCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ThreadSleepInAsyncMethodTests
{
    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            {|#0:Thread.Sleep(5000)|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Task.Delay(5000);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_StaticImport()
    {
        var original = @"
using System;
using System.Text;
using static System.Threading.Thread;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            {|#0:Sleep(5000)|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using static System.Threading.Thread;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Task.Delay(5000);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_GenericAsyncMethod_AndThreadSleep()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task<int> MyMethod()
        {
            {|#0:Thread.Sleep(5000)|};
            return 5;
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task<int> MyMethod()
        {
            await Task.Delay(5000);
            return 5;
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncVoidMethod_AndThreadSleep()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            {|#0:Thread.Sleep(5000)|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            await Task.Delay(5000);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_VoidMethod_AndThreadSleep()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        void MyMethod()
        {
            Thread.Sleep(5000);
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_TaskMethod_AndThreadSleep_NoFix()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        Task MyMethod()
        {
            {|#0:Thread.Sleep(5000)|};
            return Task.CompletedTask;
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"));
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_Constructor_AndThreadSleep()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        MyClass()
        {
            Thread.Sleep(5000);
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_Alias()
    {
        var original = @"
using System;
using System.Text;
using MyThread = System.Threading.Thread;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            {|#0:MyThread.Sleep(5000)|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using MyThread = System.Threading.Thread;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Task.Delay(5000);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AddsUsing()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            {|#0:Thread.Sleep(5000)|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void MyMethod()
        {
            await Task.Delay(5000);
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_ArrowSyntax()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod() => {|#0:Thread.Sleep(5000)|};
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod() => await Task.Delay(5000);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/23")]
    public async Task ThreadSleepInAsyncMethod_GenericMethod()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async void Method()
        {
            this.Other<string>();
        }

        void Other<T>() { }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_Async_ValueTask()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    async ValueTask MyMethod()
    {
        {|#0:Thread.Sleep(5000)|};
    }
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    async ValueTask MyMethod()
    {
        await Task.Delay(5000);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_Sync_ValueTask()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    ValueTask MyMethod()
    {
        {|#0:Thread.Sleep(5000)|};
        return ValueTask.CompletedTask;
    }
}";
        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"));
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_TopLevel()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() => {|#0:Thread.Sleep(5000)|};
await MyMethod();
";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() => await Task.Delay(5000);
await MyMethod();
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_LocalFunction()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task Method()
{
    async Task MyMethod() => {|#0:Thread.Sleep(5000)|};
    await MyMethod();
}";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task Method()
{
    async Task MyMethod() => await Task.Delay(5000);
    await MyMethod();
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_Lambda()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = () => Thread.Sleep(32);
	lambda();
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_LambdaAsync()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = async () => {|#0:Thread.Sleep(32)|};
	lambda();
}
";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() {
	Action lambda = async () => await Task.Delay(32);
	lambda();
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_LambdaAsync_InClass()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod() {
	    Action lambda = async () => {|#0:Thread.Sleep(32)|};
	    lambda();
    }
}
";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod() {
	    Action lambda = async () => await Task.Delay(32);
	    lambda();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncLambda_AsAnonymousFunction()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(async () => {|#0:Thread.Sleep(32)|});
    }
}";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(async () => await Task.Delay(32));
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_Timespan()
    {
        var original = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            {|#0:Thread.Sleep(TimeSpan.FromSeconds(5))|};
        }
    }
}";

        var result = @"
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        async Task MyMethod()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously sleeping thread in an async method"), result);
    }
}