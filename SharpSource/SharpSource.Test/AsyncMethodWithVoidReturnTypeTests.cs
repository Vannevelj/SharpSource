using System.Threading.Tasks;
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
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndTask()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_NoAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndTaskGeneric()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndEventHandlerArguments()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndEventHandlerSubClassArguments()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncDelegate()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncVoidAndArbitraryArguments()
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

        await VerifyDiagnostic(original, "Method MyHandler is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndVoid()
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

        await VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithPartialMethod()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/19")]
    public async Task AsyncMethodWithVoidReturnType_AddsUsingStatement()
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

        await VerifyDiagnostic(original, "Method Method is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_TopLevel()
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

        await VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_LocalFunction()
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

        await VerifyDiagnostic(original, "Method MyMethod is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/258")]
    public async Task AsyncMethodWithVoidReturnType_WithInterfaceImplementation()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass : SomeInterface
{   
    public async void MyMethod() => await Task.CompletedTask;
}

interface SomeInterface
{
    void MyMethod();
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithExplicitInterfaceImplementation()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass : SomeInterface
{   
    async void SomeInterface.MyMethod() => await Task.CompletedTask;
}

interface SomeInterface
{
    void MyMethod();
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithAbstractImplementation()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass : Base
{   
    public async override void MyMethod() => await Task.CompletedTask;
}

abstract class Base
{
    public abstract void MyMethod();
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithIrrelevantInterfaceImplementation()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass : SomeInterface
{
    async void RealMethod() => await Task.CompletedTask;
    public int MyMethod() => 5;
}

interface SomeInterface
{
    int MyMethod();
}";

        var result = @"
using System;
using System.Threading.Tasks;

class MyClass : SomeInterface
{
    async Task RealMethod() => await Task.CompletedTask;
    public int MyMethod() => 5;
}

interface SomeInterface
{
    int MyMethod();
}";

        await VerifyDiagnostic(original, "Method RealMethod is marked as async but has a void return type");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/259")]
    public async Task AsyncMethodWithVoidReturnType_WithAsyncAndWinUIEventHandlerArguments()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class ItemClickEventArgs { }

class MyClass
{   
    async void MyHandler(object o, ItemClickEventArgs e)
    {
        await Task.CompletedTask;
    }
}";

        await VerifyDiagnostic(original);
    }
}