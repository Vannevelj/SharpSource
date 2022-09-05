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
    public void TestMethodWithoutTestAttribute_MSTest()
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

        VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_NUnit()
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

        VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_XUnit_NoOtherMethodsWithAttribute()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_XUnit_OtherMethodWithAttribute()
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

        VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_TaskReturn()
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

        VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_TaskTReturn()
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

        VerifyDiagnostic(original, "Method MyMethod might be missing a test attribute");
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_OtherReturnType()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void TestMethodWithoutTestAttribute_OtherAttribute()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/22")]
    public void TestMethodWithoutTestAttribute_PrivateMethod()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/91")]
    [DataRow("record")]
    [DataRow("record class")]
    [DataRow("record struct")]
    public void TestMethodWithoutTestAttribute_Record(string record)
    {
        var original = $@"
{record} Test
{{
    public void MyMethod() {{ }}
}}";

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/98")]
    public void TestMethodWithoutTestAttribute_Dispose()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
class Test : IDisposable
{
    public void Dispose() { }
}";

        VerifyDiagnostic(original);
    }
}