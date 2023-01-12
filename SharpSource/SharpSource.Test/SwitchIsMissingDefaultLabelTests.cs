using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.SwitchIsMissingDefaultLabelAnalyzer, SharpSource.Diagnostics.SwitchIsMissingDefaultLabelCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class SwitchIsMissingDefaultLabelTests
{
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
            {|#0:switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }|}
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
            {|#0:switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
                default:
                    throw new ArgumentException(""Unsupported value"");
            }|}
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch (e)
            {
                case ""test"":
                case ""test1"":
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch (""test"")
            {
                case ""test"":
                case ""test1"":
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch (0)
            {
                case 0:
                case 1:
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
            {|#0:switch ((x))
            {
                case 5:
                case 6:
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch ((""test""))
            {
                case ""test"":
                case ""test1"":
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch ((0))
            {
                case 0:
                case 1:
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
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
            {|#0:switch (0)
            {
                case 0:
                case 1:
                    break;
            }|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/167")]
    public async Task SwitchIsMissingDefaultLabel_TopLevelStatement()
    {
        var original = @"
using System;

{|#0:switch (Test.A)
{
    case Test.A: return;
}|}

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
    }
}