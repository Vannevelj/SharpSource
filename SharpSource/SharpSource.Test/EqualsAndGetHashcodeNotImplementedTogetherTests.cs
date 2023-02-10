using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.EqualsAndGetHashcodeNotImplementedTogetherAnalyzer, SharpSource.Diagnostics.EqualsAndGetHashcodeNotImplementedTogetherCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class EqualsAndGetHashcodeNotImplementedTogetherTests
{
    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_BothImplemented_NoWarning()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_EqualsImplemented()
    {
        var original = @"
namespace ConsoleApplication1
{
    class {|#0:MyClass|}
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"), result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_GetHashcodeImplemented()
    {
        var original = @"
namespace ConsoleApplication1
{
    class {|#0:MyClass|}
    {
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"), result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_NeitherImplemented_NoWarning()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_NonOverridingEqualsImplemented_NoWarning()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        public bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_NonOverridingGetHashcodeImplemented_NoWarning()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        public int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_EqualsImplemented_SimplifiesNameWhenUsingSystem()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class {|#0:MyClass|}
    {
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"), result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_GetHashcodeImplemented_SimplifiesNameWhenUsingSystem()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class {|#0:MyClass|}
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"), result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_GetHashcodeImplemented_BaseClassImplementsBoth()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyBaseClass
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }

    class {|#0:MyClass|} : MyBaseClass
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    class MyBaseClass
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }

    class MyClass : MyBaseClass
    {
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"), result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_Partial()
    {
        var original = @"
partial class MyClass
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }
}

partial class MyClass
{
    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/296")]
    public async Task EqualsAndGetHashcodeNotImplemented_NullableContext()
    {
        var original = @"
class {|#0:MyClass|}
{
    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}";

        var result = @"
class MyClass
{
    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        throw new System.NotImplementedException();
    }
}";

        var test = new VerifyCS.Test
        {
            TestCode = original,
            FixedCode = result,
            NullableContextOptions = NullableContextOptions.Enable
        };

        test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic().WithMessage("Equals() and GetHashcode() must be implemented together on MyClass"));

        await test.RunAsync();
    }
}