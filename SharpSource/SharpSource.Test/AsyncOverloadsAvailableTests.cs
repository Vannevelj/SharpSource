using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class AsyncOverloadsAvailableTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AsyncOverloadsAvailableAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new AsyncOverloadsAvailableCodeFix();

    [TestMethod]
    public async Task AsyncOverloadsAvailable_WithOverload_InAsyncContext()
    {
        var original = @"
using System;
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyMethod()
        {
            new StringWriter().Write("""");
        }
    }
}";

        var result = @"
using System;
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyMethod()
        {
            await new StringWriter().WriteAsync("""");
        }
    }
}";

        await VerifyDiagnostic(original, "Async overload available for StringWriter.Write");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_WithOverload_InSyncContext()
    {
        var original = @"
using System;
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {   
        void MyMethod()
        {
            new StringWriter().Write("""");
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_WithoutOverload_InAsyncContext()
    {
        var original = @"
using System;
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyMethod()
        {
            Console.Write("""");
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_AsyncMethod_InAsyncContext()
    {
        var original = @"
using System;
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void MyMethod()
        {
            new StringWriter().WriteAsync("""");
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
    public async Task AsyncOverloadsAvailable_DifferentReturnType()
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
            Get();
        }

        string Get() => null;

        async Task<int> GetAsync() => 5;
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
    public async Task AsyncOverloadsAvailable_InCurrentType()
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
            Get();
        }

        string Get() => null;

        async Task<string> GetAsync() => null;
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
            await GetAsync();
        }

        string Get() => null;

        async Task<string> GetAsync() => null;
    }
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
    public async Task AsyncOverloadsAvailable_Void()
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
            Do();
        }

        void Do() { }

        async Task DoAsync() { }
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
            await DoAsync();
        }

        void Do() { }

        async Task DoAsync() { }
    }
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Do");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/25")]
    public async Task AsyncOverloadsAvailable_DifferentParameters()
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
            Get();
        }

        string Get() => null;

        async Task<string> GetAsync(int a) => null;
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_DifferentParameters_OptionalCancellationToken()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task MyMethod()
        {
            Get();
        }

        string Get() => null;

        async Task<string> GetAsync(CancellationToken? token = null) => null;
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
        async Task MyMethod()
        {
            await GetAsync();
        }

        string Get() => null;

        async Task<string> GetAsync(CancellationToken? token = null) => null;
    }
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_DifferentParameters_MandatoryCancellationToken()
    {
        var original = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async Task MyMethod()
        {
            Get();
        }

        string Get() => null;

        async Task<string> GetAsync(CancellationToken token) => null;
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/24")]
    public async Task AsyncOverloadsAvailable_GenericMethod()
    {
        var original = @"
using System;
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

    [TestMethod]
    public async Task AsyncOverloadsAvailable_GenericOverload()
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
            Get<string>();
        }

        T Get<T>() => default(T);

        async Task<T> GetAsync<T>() => default(T);
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
            await GetAsync<string>();
        }

        T Get<T>() => default(T);

        async Task<T> GetAsync<T>() => default(T);
    }
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/26")]
    public async Task AsyncOverloadsAvailable_OverloadWithLessParameters()
    {
        var original = @"
using System;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class MyClass
    {   
        async void Method()
        {
            Other(32);
        }

        void Other(int a) { }
        void OtherAsync() { }
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/88")]
    public async Task AsyncOverloadsAvailable_WithOverload_AccessingReturnValue()
    {
        var original = @"
using System.Threading.Tasks;

class Test
{
    string DoThing() => string.Empty;
    async Task<string> DoThingAsync() => string.Empty;

    async Task Method()
    {
        var length = DoThing().Length;
    }
}";

        var result = @"
using System.Threading.Tasks;

class Test
{
    string DoThing() => string.Empty;
    async Task<string> DoThingAsync() => string.Empty;

    async Task Method()
    {
        var length = (await DoThingAsync()).Length;
    }
}";

        await VerifyDiagnostic(original, "Async overload available for Test.DoThing");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/97")]
    public async Task AsyncOverloadsAvailable_WithOverload_OnlyIfOverloadIsFound()
    {
        var original = @"
using System.Threading.Tasks;
using System;

class Test
{
    async Task Method()
    {
        try { }
        catch (Exception e)
        {
            System.Console.Error.WriteLine(e);
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/103")]
    public async Task AsyncOverloadsAvailable_WithOverload_GlobalStatement()
    {
        var original = @"
using System.IO;

new StringWriter().Write(string.Empty);";

        var result = @"
using System.IO;

await new StringWriter().WriteAsync(string.Empty);";

        await VerifyDiagnostic(original, "Async overload available for StringWriter.Write");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/104")]
    public async Task AsyncOverloadsAvailable_WithOverload_Nullability()
    {
        var original = @"
#nullable enable
using System.Threading.Tasks;

Test.Method(null);

class Test
{
    public static void Method(string? arg) { }
    public static async Task MethodAsync(string arg) { }
}
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/110")]
    public async Task AsyncOverloadsAvailable_DifferentReturnType_ValueTask()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod()
    {
        Get();
    }

    int Get() => 5;

    async ValueTask<int> GetAsync() => 5;
}";

        var result = @"
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod()
    {
        await GetAsync();
    }

    int Get() => 5;

    async ValueTask<int> GetAsync() => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/120")]
    public async Task AsyncOverloadsAvailable_DoesNotSuggestItself()
    {
        var original = @"
using System.Threading.Tasks;

class Test
{
    private void CleanAssets() { }

    private async Task CleanAssetsAsync()
    {
        CleanAssets();
        await Task.Delay(1);
    }
}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/120")]
    public async Task AsyncOverloadsAvailable_SyncLambda()
    {
        var original = @"
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

class Test
{
	int DoThing() => 5;
	async Task<int> DoThingAsync() => 5;

	async Task Method()
	{
		var obj = new List<Test>().Select(t => t.DoThing());
	}
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_AsyncLambda_WithMatch()
    {
        var original = @"
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

class Test
{
	public int DoThing() => 5;
	public async Task<int> DoThingAsync() => 5;

	async Task Method()
	{
		var obj = new List<Test>().Select(async t => t.DoThing());
	}
}";

        var result = @"
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

class Test
{
	public int DoThing() => 5;
	public async Task<int> DoThingAsync() => 5;

	async Task Method()
	{
		var obj = new List<Test>().Select(async t => await t.DoThingAsync());
	}
}";

        await VerifyDiagnostic(original, "Async overload available for Test.DoThing");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_AsyncLambda_WithoutMatch()
    {
        var original = @"
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

class Test
{
	int DoThing() => 5;
	async Task DoThingAsync() { }

	async Task Method()
	{
		var obj = new List<Test>().Select(async t => t.DoThing());
	}
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        Get();
    }

    int Get() => 5;
    async Task<int> GetAsync(CancellationToken token) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        await GetAsync(token);
    }

    int Get() => 5;
    async Task<int> GetAsync(CancellationToken token) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_Optional()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken? token = null)
    {
        Get();
    }

    int Get() => 5;
    async Task<int> GetAsync(CancellationToken token) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken? token = null)
    {
        await GetAsync(token ?? CancellationToken.None);
    }

    int Get() => 5;
    async Task<int> GetAsync(CancellationToken token) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_OtherParameters()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        Get(5);
    }

    int Get(int i) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        await GetAsync(5, token);
    }

    int Get(int i) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_MultipleParameters()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        Get(32, string.Empty, true);
    }

    int Get(int i, string y, bool z) => 5;
    async Task<int> GetAsync(int i, string y, bool z) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        await GetAsync(32, string.Empty, true);
    }

    int Get(int i, string y, bool z) => 5;
    async Task<int> GetAsync(int i, string y, bool z) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_AlreadyPassingThrough()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        Get(5, token);
    }

    int Get(int i, CancellationToken token) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        await GetAsync(5, token);
    }

    int Get(int i, CancellationToken token) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_LocalFunction()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        int Get(int i, CancellationToken token) => 5;
        async Task<int> GetAsync(int x, CancellationToken token) => 5;

        Get(5, token);
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_NotAsLastParameter_InCurrentMethod()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(string i, CancellationToken token, int x)
    {
        Get(5);
    }

    int Get(int i) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(string i, CancellationToken token, int x)
    {
        await GetAsync(5, token);
    }

    int Get(int i) => 5;
    async Task<int> GetAsync(int x, CancellationToken token) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_NotAsLastParameter_InCalledMethod()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(string i, CancellationToken token, int x)
    {
        Get(5, x);
    }

    int Get(int i, int x) => 5;
    async Task<int> GetAsync(int x, CancellationToken token, int y) => 5;
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AsyncOverloadsAvailable_PassesThroughCancellationToken_SyncOverloadAcceptsCancellationToken()
    {
        var original = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        Get();
    }

    int Get(CancellationToken? token = null) => 5;
    async Task<int> GetAsync(CancellationToken? token = null) => 5;
}";

        var result = @"
using System.Threading;
using System.Threading.Tasks;

class MyClass
{   
    async Task MyMethod(CancellationToken token)
    {
        await GetAsync(token);
    }

    int Get(CancellationToken? token = null) => 5;
    async Task<int> GetAsync(CancellationToken? token = null) => 5;
}";

        await VerifyDiagnostic(original, "Async overload available for MyClass.Get");
        await VerifyFix(original, result);
    }
}