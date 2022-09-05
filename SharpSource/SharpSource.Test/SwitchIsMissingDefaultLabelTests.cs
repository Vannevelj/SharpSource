using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class SwitchIsMissingDefaultLabelTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SwitchIsMissingDefaultLabelAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new SwitchIsMissingDefaultLabelCodeFix();

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnEnum()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnString()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnStringLiteral()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnIntegerLiteral()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_HasDefaultStatement()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_ParenthesizedStatement()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnParenthsizedStringLiteral()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_SwitchOnParenthsizedIntegerLiteral()
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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void SwitchIsMissingDefaultLabel_MissingDefaultStatement_AddsUsingStatement()
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

        var result = @"using System;

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

        VerifyDiagnostic(original, "Switch should have default label.");
        VerifyFix(original, result);
    }
}