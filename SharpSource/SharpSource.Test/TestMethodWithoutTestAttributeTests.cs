using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.TestMethodWithoutTestAttributeAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class TestMethodWithoutTestAttributeTests
{
    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_MSTest()
    {
        var original = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public void {|#0:MyMethod|}()
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod might be missing a test attribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_NUnit()
    {
        var original = @"
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    class MyClass
    {
        public void {|#0:MyMethod|}()
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod might be missing a test attribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_XUnit_NoOtherMethodsWithAttribute()
    {
        var original = @"
using Xunit;

namespace ConsoleApplication1
{
    class MyClass
    {
        public void MyMethod()
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_XUnit_OtherMethodWithAttribute()
    {
        var original = @"
using Xunit;

namespace ConsoleApplication1
{
    class MyClass
    {
        public void {|#0:MyMethod|}()
        {
        }

        [Fact]
        public void MyOtherMethod()
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod might be missing a test attribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_TaskReturn()
    {
        var original = @"
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public async Task {|#0:MyMethod|}()
        {
            await Task.Delay(0);
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod might be missing a test attribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_TaskTReturn()
    {
        var original = @"
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public Task<int> {|#0:MyMethod|}()
        {
            return Task.FromResult(5);
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Method MyMethod might be missing a test attribute"));
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_OtherReturnType()
    {
        var original = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public int MyMethod()
        {
            return 5;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_OtherAttribute()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        [SomethingElse]
        public int MyMethod()
        {
            return 5;
        }
    }
}

class SomethingElseAttribute : Attribute { }
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/22")]
    public async Task TestMethodWithoutTestAttribute_PrivateMethod()
    {
        var original = @"
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        private void MyMethod()
        {

        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/91")]
    [DataRow("record")]
    [DataRow("record class")]
    [DataRow("record struct")]
    public async Task TestMethodWithoutTestAttribute_Record(string record)
    {
        var original = $@"
{record} Test
{{
    public void MyMethod() {{ }}
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/98")]
    public async Task TestMethodWithoutTestAttribute_Dispose()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
class Test : IDisposable
{
    public void Dispose() { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/255")]
    public async Task TestMethodWithoutTestAttribute_LifetimeHooks()
    {
        var original = @"
using Xunit;
using System.Threading.Tasks;

public class MyClass : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
    }

    public async Task DisposeAsync() { }

    [Fact]
    public void MyOtherMethod()
    {
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_Struct()
    {
        var original = @"
using Xunit;

struct MyClass
{
    [Fact]
    public void MyMethod()
    {
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_Constructor()
    {
        var original = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MyClass
{
    public MyClass()
    {
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_DisposeVirtual()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class TestBase : IDisposable
{
    public virtual void Dispose() { }
}

[TestClass]
public class MyClass : TestBase
{
    public override void Dispose() => base.Dispose();
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}