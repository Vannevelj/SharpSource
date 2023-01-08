using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.NewGuidAnalyzer, SharpSource.Diagnostics.NewGuidCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class NewGuidTests
{
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
            var g = {|#0:new Guid()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result);
    }

    [TestMethod]
    public async Task NewGuid_Constructor_NewGuid_Implicit()
    {
        var original = @"
using System;
void Method()
{
    Guid g = {|#0:new()|};
}";

        var result = @"
using System;
void Method()
{
    Guid g = Guid.NewGuid();
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result);
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
            var g = {|#0:new System.Guid()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result);
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
            var g = {|#0:new Guid()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result, codeActionIndex: 1);
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
            var g = {|#0:new System.Guid()|};
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result, codeActionIndex: 1);
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
            Console.WriteLine({|#0:new Guid()|});
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An empty guid was created in an ambiguous manner"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}