using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class AccessingTaskResultWithoutAwaitTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AccessingTaskResultWithoutAwaitAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new AccessingTaskResultWithoutAwaitCodeFix();

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_AsyncContext()
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
            var number = Other().Result;
        }

        async Task<int> Other() => 5;
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
            var number = await Other();
        }

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_SyncContext()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        Task MyMethod()
        {
            var number = Other().Result;
            return Task.CompletedTask;
        }

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_AsyncContext_Void()
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
            var number = Other().Result;
        }

        async Task<int> Other() => 5;
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
            var number = await Other();
        }

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_ExpressionBodiedMember()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task<int> MyMethod() => Other().Result;

        async Task<int> Other() => 5;
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
        async Task<int> MyMethod() => await Other();

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_ChainedInvocations()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task<int> MyMethod() => new MyClass().Other().Result;

        async Task<int> Other() => 5;
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
        async Task<int> MyMethod() => await new MyClass().Other();

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_AsyncLambda()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(async () => Console.Write(Other().Result));

	    async Task<int> Other() => 5;
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
	    Action MyMethod() => new Action(async () => Console.Write(await Other()));

	    async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_SyncLambda()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
	    Action MyMethod() => new Action(() => Console.Write(Other().Result));

	    async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_Constructor()
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
            var number = Other().Result;
        }

        async Task<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_ChainedPropertyAccess()
    {
        var original = @"
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {
        public int SomeField;

        async Task MyMethod()
        {
            var number = Other().Result.SomeField;
        }

        async Task<MyClass> Other() => this;
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
        public int SomeField;

        async Task MyMethod()
        {
            var number = (await Other()).SomeField;
        }

        async Task<MyClass> Other() => this;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_ObjectInitializer()
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
		    Console.Write(new {
			    Prop = Get().Result
		    });
	    }
	
	    async Task<int> Get() => 5;
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
		    Console.Write(new {
			    Prop = await Get()
            });
	    }
	
	    async Task<int> Get() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/111")]
    public async Task AccessingTaskResultWithoutAwait_AsyncContext_ValueTask()
    {
        var original = @"
using System;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task MyMethod()
        {
            var number = Other().Result;
        }

        async ValueTask<int> Other() => 5;
    }
}";

        var result = @"
using System;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task MyMethod()
        {
            var number = await Other();
        }

        async ValueTask<int> Other() => 5;
    }
}";

        await VerifyDiagnostic(original, "Use await to get the result of a Task.");
        await VerifyFix(original, result);
    }
}