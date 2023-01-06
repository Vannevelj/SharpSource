using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class SwitchIsMissingDefaultLabelTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SwitchIsMissingDefaultLabelAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new SwitchIsMissingDefaultLabelCodeFix();

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnEnum()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
using System;

namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnString()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = ""test"";
            switch (e)
            {
                case ""test"":
                case ""test1"":
                    break;
            }
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
            var e = ""test"";
            switch (e)
            {
                case ""test"":
                case ""test1"":
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnStringLiteral()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch (""test"")
            {
                case ""test"":
                case ""test1"":
                    break;
            }
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
            switch (""test"")
            {
                case ""test"":
                case ""test1"":
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnIntegerLiteral()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch (0)
            {
                case 0:
                case 1:
                    break;
            }
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
            switch (0)
            {
                case 0:
                case 1:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_HasDefaultStatement()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = ""test"";
            switch (e)
            {
                case ""test"":
                case ""test1"":
                default:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_ParenthesizedStatement()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var x = 5;
            switch ((x))
            {
                case 5:
                case 6:
                    break;
            }
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
            var x = 5;
            switch ((x))
            {
                case 5:
                case 6:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnParenthsizedStringLiteral()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch ((""test""))
            {
                case ""test"":
                case ""test1"":
                    break;
            }
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
            switch ((""test""))
            {
                case ""test"":
                case ""test1"":
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnParenthesizedIntegerLiteral()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch ((0))
            {
                case 0:
                case 1:
                    break;
            }
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
            switch ((0))
            {
                case 0:
                case 1:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_MissingDefaultStatement_AddsUsingStatement()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch (0)
            {
                case 0:
                case 1:
                    break;
            }
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
            switch (0)
            {
                case 0:
                case 1:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }
        }
    }
}";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/167")]
    public async Task SwitchIsMissingDefaultLabel_TopLevelStatement()
    {
        var original = @"
using System;

switch (Test.A)
{
    case Test.A: return;
}

enum Test { A, B }";

        var result = @"
using System;

switch (Test.A)
{
    case Test.A: return;
    default:
        throw new ArgumentException(""Unsupported value"");
}

enum Test { A, B }";

        await VerifyDiagnostic(original, "Switch should have default label.");
        await VerifyFix(original, result);
    }
}