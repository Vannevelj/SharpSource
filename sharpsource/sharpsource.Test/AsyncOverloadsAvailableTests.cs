using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynTester.Helpers.CSharp;
using SharpSource.Diagnostics.CorrectTPLMethodsInAsyncContext;
using SharpSource.Tests.Helpers;

namespace SharpSource.Tests
{
    [TestClass]
    public class AsyncOverloadsAvailableTests : CSharpCodeFixVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AsyncOverloadsAvailableAnalyzer();

        protected override CodeFixProvider CodeFixProvider => new AsyncOverloadsAvailableCodeFix();

        [TestMethod]
        public void AsyncOverloadsAvailable_WithOverload_InAsyncContext()
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

            VerifyDiagnostic(original, "Async overload available for StringWriter.Write");
            VerifyFix(original, result);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_WithOverload_InSyncContext()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_WithoutOverload_InAsyncContext()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_AsyncMethod_InAsyncContext()
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

            VerifyDiagnostic(original);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
        public void AsyncOverloadsAvailable_DifferentReturnType()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_InCurrentType()
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

            VerifyDiagnostic(original, "Async overload available for MyClass.Get");
            VerifyFix(original, result);
        }
    }
}
