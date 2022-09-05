using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ThreadSleepInAsyncMethodTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ThreadSleepInAsyncMethodAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new ThreadSleepInAsyncMethodCodeFix();

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleepAsync()
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
            Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_StaticImportAsync()
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
            Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_GenericAsyncMethod_AndThreadSleepAsync()
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
            Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncVoidMethod_AndThreadSleepAsync()
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
            Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_VoidMethod_AndThreadSleepAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_TaskMethod_AndThreadSleep_NoFixAsync()
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
            Thread.Sleep(5000);
            return Task.CompletedTask;
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
        Task MyMethod()
        {
            Thread.Sleep(5000);
            return Task.CompletedTask;
        }
    }
}";

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_Constructor_AndThreadSleepAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AliasAsync()
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
            MyThread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AddsUsingAsync()
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
            Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_ArrowSyntaxAsync()
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
        async Task MyMethod() => Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/23")]
    public async Task ThreadSleepInAsyncMethod_GenericMethodAsync()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_Async_ValueTaskAsync()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    async ValueTask MyMethod()
    {
        Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_Sync_ValueTaskAsync()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    ValueTask MyMethod()
    {
        Thread.Sleep(5000);
        return ValueTask.CompletedTask;
    }
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class Test
{
    ValueTask MyMethod()
    {
        Thread.Sleep(5000);
        return ValueTask.CompletedTask;
    }
}";

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_TopLevelAsync()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() => Thread.Sleep(5000);
await MyMethod();
";

        var result = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task MyMethod() => await Task.Delay(5000);
await MyMethod();
";

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_LocalFunctionAsync()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

async Task Method()
{
    async Task MyMethod() => Thread.Sleep(5000);
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

        await VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ThreadSleepInAsyncMethod_AsyncMethod_LambdaAsync()
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

        await VerifyDiagnostic(original);
    }
}