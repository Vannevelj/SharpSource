using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynTester.Helpers.CSharp;
using SharpSource.Diagnostics.AccessingTaskResultWithoutAwait;
using SharpSource.Diagnostics.AsyncMethodWithVoidReturnType;
using SharpSource.Tests.Helpers;

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
    }
}
