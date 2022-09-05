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
    public async Task SynchronousTaskWait_AsyncContextAsync()
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
    public async Task SynchronousTaskWait_SyncContextAsync()
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
    public async Task SynchronousTaskWait_AsyncContext_VoidAsync()
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
    public async Task SynchronousTaskWait_ExpressionBodiedMemberAsync()
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
    public async Task SynchronousTaskWait_AsyncLambdaAsync()
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
    public async Task SynchronousTaskWait_SyncLambdaAsync()
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
    public async Task SynchronousTaskWait_ConstructorAsync()
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
    public async Task SynchronousTaskWait_ChainedExpressionAsync()
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
}