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

            VerifyDiagnostic(original, "Asynchronously await tasks instead of blocking them");
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

            VerifyDiagnostic(original, "Asynchronously await tasks instead of blocking them");
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

            VerifyDiagnostic(original, "Asynchronously await tasks instead of blocking them");
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

            VerifyDiagnostic(original, "Asynchronously await tasks instead of blocking them");
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
            var number = Other().Wait();
        }

        async Task<int> Other() => 5;
    }
}";

            VerifyDiagnostic(original);
        }
    }
}
