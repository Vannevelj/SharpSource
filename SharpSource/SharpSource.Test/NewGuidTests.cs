using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class NewGuidTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new NewGuidAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new NewGuidCodeFix();

    [TestMethod]
    public async Task NewGuid_Constructor_NewGuid()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = new Guid();
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = Guid.NewGuid();
        }
    }
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 0);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_NewGuid_Implicit()
    {
        var original = @"
using System;
void Method()
{
    Guid g = new();
}";

        var result = @"
using System;
void Method()
{
    Guid g = Guid.NewGuid();
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 0);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_NewGuid_FullName()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = new System.Guid();
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = System.Guid.NewGuid();
        }
    }
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 0);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_EmptyGuid()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = new Guid();
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = Guid.Empty;
        }
    }
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 1);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_EmptyGuid_FullName()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = new System.Guid();
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var g = System.Guid.Empty;
        }
    }
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 1);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_AsExpression()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Console.WriteLine(new Guid());
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Console.WriteLine(Guid.NewGuid());
        }
    }
}";

        await VerifyDiagnostic(original, "An empty guid was created in an ambiguous manner");
        await VerifyFix(original, result, codeFixIndex: 0);
    }

    [TestMethod]
    public async Task NewGuid_GuidNewGuid()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Guid g = Guid.NewGuid();
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task NewGuid_GuidEmpty()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Guid g = Guid.Empty;
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task NewGuid_Default()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Guid g = default(Guid);
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task NewGuid_OverloadedConstructor()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            Guid g = new Guid(string.Empty);
        }
    }
}";

        await VerifyDiagnostic(original);
    }
}