using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test
{
    [TestClass]
    public class ThreadSleepInAsyncMethodTests : CSharpCodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ThreadSleepInAsyncMethodAnalyzer();

        protected override CodeFixProvider CodeFixProvider => new ThreadSleepInAsyncMethodCodeFix();

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_StaticImport()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_GenericAsyncMethod_AndThreadSleep()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncVoidMethod_AndThreadSleep()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_VoidMethod_AndThreadSleep()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_TaskMethod_AndThreadSleep_NoFix()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_Constructor_AndThreadSleep()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_Alias()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AddsUsing()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_AndThreadSleep_ArrowSyntax()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/23")]
        public void ThreadSleepInAsyncMethod_GenericMethod()
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

            VerifyDiagnostic(original);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
        public void ThreadSleepInAsyncMethod_AsyncMethod_Async_ValueTask()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/112")]
        public void ThreadSleepInAsyncMethod_AsyncMethod_Sync_ValueTask()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_TopLevel()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_LocalFunction()
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

            VerifyDiagnostic(original, "Synchronously sleeping thread in an async method");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void ThreadSleepInAsyncMethod_AsyncMethod_Lambda()
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

            VerifyDiagnostic(original);
        }
    }
}