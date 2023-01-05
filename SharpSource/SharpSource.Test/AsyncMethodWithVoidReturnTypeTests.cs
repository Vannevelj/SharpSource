using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.AsyncMethodWithVoidReturnTypeAnalyzer, SharpSource.Diagnostics.AsyncMethodWithVoidReturnTypeCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class AsyncMethodWithVoidReturnTypeTests
{
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
        async void {|#0:MyHandler|}(object o, int e)
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method MyHandler is marked as async but has a void return type"), result);
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
        async void {|#0:MyMethod|}()
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod is marked as async but has a void return type"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
        async void {|#0:Method|}()
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method Method is marked as async but has a void return type"), result);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_TopLevel()
    {
        var original = @"
using System;
using System.Threading.Tasks;

async void {|#0:MyMethod|}()
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod is marked as async but has a void return type"), result);
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
        async void {|#0:MyMethod|}()
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod is marked as async but has a void return type"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncMethodWithVoidReturnType_WithIrrelevantInterfaceImplementation()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass : SomeInterface
{
    async void {|#0:RealMethod|}() => await Task.CompletedTask;
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Method RealMethod is marked as async but has a void return type"), result);
    }
}