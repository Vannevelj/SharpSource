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
            switch ({|#0:e|})
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
            switch ({|#0:e|})
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
            switch ({|#0:e|})
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
            switch ({|#0:""test""|})
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
            switch ({|#0:0|})
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
            switch (({|#0:x|}))
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
            switch (({|#0:""test""|}))
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
            switch (({|#0:0|}))
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
            switch ({|#0:0|})
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/167")]
    public async Task SwitchIsMissingDefaultLabel_TopLevelStatement()
    {
        var original = @"
using System;

switch ({|#0:Test.A|})
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
    }

    [TestMethod]
    public async Task SwitchIsMissingDefaultLabel_SwitchExpression_WithoutDiscardArm()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        int Method(MyEnum e) => {|#0:e|} switch
        {
            MyEnum.Fizz => 1,
            MyEnum.Buzz => 2
        };
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_BoolSwitch_BothBranchesCovered_NoDiagnostic()
    {
        var original = @"
class Test
{
    void Method(bool b)
    {
        switch (b)
        {
            case true:
                break;
            case false:
                break;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_BoolSwitch_OnlyOneBranch_Diagnostic()
    {
        var original = @"
using System;
class Test
{
    void Method(bool b)
    {
        switch ({|#0:b|})
        {
            case true:
                break;
        }
    }
}";

        var result = @"
using System;
class Test
{
    void Method(bool b)
    {
        switch (b)
        {
            case true:
                break;
            default:
                throw new ArgumentException(""Unsupported value"");
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Switch should have default label."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_BoolSwitchExpression_BothBranchesCovered_NoDiagnostic()
    {
        var original = @"
class Test
{
    int Method(bool b) => b switch
    {
        true => 1,
        false => 0
    };
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_PatternMatching_TypePatterns_NoDiagnostic()
    {
        var original = @"
class Base { }
class Derived1 : Base { }
class Derived2 : Base { }

class Test
{
    void Method(Base b)
    {
        switch (b)
        {
            case Derived1 d1:
                break;
            case Derived2 d2:
                break;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_PatternMatching_TypePatternWithoutVariable_NoDiagnostic()
    {
        var original = @"
class Base { }
class Derived1 : Base { }
class Derived2 : Base { }

class Test
{
    string Method(Base b)
    {
        switch (b)
        {
            case Derived1:
                return ""one"";
            case Derived2:
                return ""two"";
        }
        return ""other"";
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/378")]
    public async Task SwitchIsMissingDefaultLabel_PatternMatching_MixedConstantAndTypePattern_NoDiagnostic()
    {
        var original = @"
class Test
{
    void Method(object o)
    {
        switch (o)
        {
            case null:
                break;
            case string s:
                break;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}