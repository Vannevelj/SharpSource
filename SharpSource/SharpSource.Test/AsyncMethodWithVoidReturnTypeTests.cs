using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class AsyncMethodWithVoidReturnTypeTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AsyncMethodWithVoidReturnTypeAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new AsyncMethodWithVoidReturnTypeCodeFix();

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncAndTask()
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
            await Task.Run(() => { });
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_NoAsync()
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

        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncAndTaskGeneric()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task<int> MyMethod()
        {
            return 32;
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncAndEventHandlerArguments()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyHandler(object o, EventArgs e)
        {
            await Task.Run(() => { });
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncAndEventHandlerSubClassArguments()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyHandler(object o, MyEventArgs e)
        {
            await Task.Run(() => { });
        }
    }

    class MyEventArgs : EventArgs 
    {

    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncDelegate()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        public void MyMethod()
        {
	        TestMethod(async () => await Task.Run(() => {}));
        }

        public void TestMethod(Action callback)
        {
	        callback();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncVoidAndArbitraryArguments()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyHandler(object o, int e)
        {
            await Task.Run(() => { });
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
        async Task MyHandler(object o, int e)
        {
            await Task.Run(() => { });
        }
    }
}";

        VerifyDiagnostic(original, "Method MyHandler is marked as async but has a void return type");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithAsyncAndVoid()
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
            await Task.Run(() => { });
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
            await Task.Run(() => { });
        }
    }
}";

        VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_WithPartialMethod()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    partial class A
    {
        partial void OnSomethingHappened();
    }

    partial class A
    {
        async partial void OnSomethingHappened()
        {
            await Task.Run(() => { });
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/19")]
    public void AsyncMethodWithVoidReturnType_AddsUsingStatement()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void Method()
        {
               
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
        async Task Method()
        {
               
        }
    }
}";

        VerifyDiagnostic(original, "Method Method is marked as async but has a void return type");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_TopLevel()
    {
        var original = @"
using System;
using System.Threading.Tasks;

async void MyMethod()
{
    await Task.CompletedTask;
}";

        var result = @"
using System;
using System.Threading.Tasks;

async Task MyMethod()
{
    await Task.CompletedTask;
}";

        VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void AsyncMethodWithVoidReturnType_LocalFunction()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class Test
{
    void Method()
    {
        async void MyMethod()
        {
            await Task.CompletedTask;
        }
    }
}";

        var result = @"
using System;
using System.Threading.Tasks;

class Test
{
    void Method()
    {
        async Task MyMethod()
        {
            await Task.CompletedTask;
        }
    }
}";

        VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        VerifyFix(original, result);
    }
}