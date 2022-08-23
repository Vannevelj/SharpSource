using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test
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

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
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

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/21")]
        public void AsyncOverloadsAvailable_Void()
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

            VerifyDiagnostic(original, "Async overload available for MyClass.Do");
            VerifyFix(original, result);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/25")]
        public void AsyncOverloadsAvailable_DifferentParameters()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_DifferentParameters_OptionalCancellationToken()
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

            VerifyDiagnostic(original, "Async overload available for MyClass.Get");
            VerifyFix(original, result);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/24")]
        public void AsyncOverloadsAvailable_GenericMethod()
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

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void AsyncOverloadsAvailable_GenericOverload()
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

            VerifyDiagnostic(original, "Async overload available for MyClass.Get");
            VerifyFix(original, result);
        }

        [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/26")]
        public void AsyncOverloadsAvailable_OverloadWithLessParameters()
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

            VerifyDiagnostic(original);
        }
    }
}