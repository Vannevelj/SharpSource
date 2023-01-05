using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.AccessingTaskResultWithoutAwaitAnalyzer, SharpSource.Diagnostics.AccessingTaskResultWithoutAwaitCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class AccessingTaskResultWithoutAwaitTests
{
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
            var number = {|#0:Other().Result|};
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
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
            var number = {|#0:Other().Result|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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
        async Task<int> MyMethod() => {|#0:Other().Result|};

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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
        async Task<int> MyMethod() => {|#0:new MyClass().Other().Result|};

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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
	    Action MyMethod() => new Action(async () => Console.Write({|#0:Other().Result|}));

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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

        await VerifyCS.VerifyCodeFix(original, original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
            var number = {|#0:Other().Result|}.SomeField;
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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
			    Prop = {|#0:Get().Result|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
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
            var number = {|#0:Other().Result|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."), result);
    }

    [TestMethod]
    public async Task AccessingTaskResultWithoutAwait_TopLevelFunction()
    {
        var original = @"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

await DoThing(File.Open("""", FileMode.Open));

async Task DoThing(FileStream file) 
{
    var result = {|#0:file.ReadAsync(new byte[] {}, 0, 0, CancellationToken.None).Result|};
}";

        var result = @"
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

await DoThing(File.Open("""", FileMode.Open));

async Task DoThing(FileStream file) 
{
    var result = await file.ReadAsync(new byte[] {}, 0, 0, CancellationToken.None);
}";
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { original },
                OutputKind = OutputKind.ConsoleApplication,
            },
            FixedCode = result,
            ExpectedDiagnostics =
            {
                VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."),
            },
        }.RunAsync();
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/146")]
    public async Task AccessingTaskResultWithoutAwait_NullableAccess_DoesNotSuggestFix()
    {
        var original = @"
#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

await DoThing(File.Open("""", FileMode.Open));

async Task DoThing(FileStream? file) 
{
    var result = file?{|#0:.ReadAsync(new byte[] {}, 0, 0, CancellationToken.None).Result|};
}";

        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { original },
                OutputKind = OutputKind.ConsoleApplication,
            },
            FixedCode = original,
            ExpectedDiagnostics =
            {
                VerifyCS.Diagnostic().WithMessage("Use await to get the result of a Task."),
            },
        }.RunAsync();
    }
}