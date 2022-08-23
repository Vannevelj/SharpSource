using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test
{
    [TestClass]
    public class SynchronousTaskWaitTests : CSharpCodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SynchronousTaskWaitAnalyzer();

        protected override CodeFixProvider CodeFixProvider => new SynchronousTaskWaitCodeFix();

        [TestMethod]
        public void SynchronousTaskWait_AsyncContext()
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

            VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void SynchronousTaskWait_SyncContext()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void SynchronousTaskWait_AsyncContext_Void()
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

            VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void SynchronousTaskWait_ExpressionBodiedMember()
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

            VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void SynchronousTaskWait_AsyncLambda()
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

            VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void SynchronousTaskWait_SyncLambda()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void SynchronousTaskWait_Constructor()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void SynchronousTaskWait_ChainedExpression()
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

            VerifyDiagnostic(original, "Asynchronously wait for task completion using await instead");
            VerifyFix(original, result);
        }
    }
}