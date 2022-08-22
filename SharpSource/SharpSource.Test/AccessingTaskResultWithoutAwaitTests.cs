using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynTester.Helpers.CSharp;
using SharpSource.Diagnostics;

namespace SharpSource.Tests
{
    [TestClass]
    public class AccessingTaskResultWithoutAwaitTests : CSharpCodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AccessingTaskResultWithoutAwaitAnalyzer();

        protected override CodeFixProvider CodeFixProvider => new AccessingTaskResultWithoutAwaitCodeFix();

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_AsyncContext()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_SyncContext()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_AsyncContext_Void()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_ExpressionBodiedMember()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_ChainedInvocations()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_AsyncLambda()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_SyncLambda()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_Constructor()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_ChainedPropertyAccess()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AccessingTaskResultWithoutAwait_ObjectInitializer()
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

            VerifyDiagnostic(original, "Use await to get the result of a Task.");
            VerifyFix(original, result);
        }
    }
}