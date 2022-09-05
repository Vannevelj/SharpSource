using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class TestMethodWithoutTestAttributeTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new TestMethodWithoutTestAttributeAnalyzer();

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_MSTestAsync()
    {
        var original = @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public void MyMethod()
        {
        }
    }
}";

        await VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_NUnitAsync()
    {
        var original = @"
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    class MyClass
    {
        public void MyMethod()
        {
        }
    }
}";

        await VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_XUnit_NoOtherMethodsWithAttributeAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_XUnit_OtherMethodWithAttributeAsync()
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

        [Fact]
        public void MyOtherMethod()
        {
        }
    }
}";

        await VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_TaskReturnAsync()
    {
        var original = @"
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public async Task MyMethod()
        {
            await Task.Delay(0);
        }
    }
}";

        await VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_TaskTReturnAsync()
    {
        var original = @"
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    class MyClass
    {
        public Task<int> MyMethod()
        {
            return Task.FromResult(5);
        }
    }
}";

        await VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_OtherReturnTypeAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutTestAttribute_OtherAttributeAsync()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/22")]
    public async Task TestMethodWithoutTestAttribute_PrivateMethodAsync()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/91")]
    [DataRow("record")]
    [DataRow("record class")]
    [DataRow("record struct")]
    public async Task TestMethodWithoutTestAttribute_RecordAsync(string record)
    {
        var original = $@"
{record} Test
{{
    public void MyMethod() {{ }}
}}";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/98")]
    public async Task TestMethodWithoutTestAttribute_DisposeAsync()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
class Test : IDisposable
{
    public void Dispose() { }
}";

        await VerifyDiagnostic(original);
    }
}