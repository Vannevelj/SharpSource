using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class EqualsAndGetHashcodeNotImplementedTogetherTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new EqualsAndGetHashcodeNotImplementedTogetherAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new EqualsAndGetHashcodeNotImplementedTogetherCodeFix();

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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_EqualsImplemented()
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

        await VerifyDiagnostic(original, "Equals() and GetHashcode() must be implemented together on MyClass");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_GetHashcodeImplemented()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
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

        await VerifyDiagnostic(original, "Equals() and GetHashcode() must be implemented together on MyClass");
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_EqualsImplemented_SimplifiesNameWhenUsingSystem()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
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

        await VerifyDiagnostic(original, "Equals() and GetHashcode() must be implemented together on MyClass");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_GetHashcodeImplemented_SimplifiesNameWhenUsingSystem()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    class MyClass
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

        await VerifyDiagnostic(original, "Equals() and GetHashcode() must be implemented together on MyClass");
        await VerifyFix(original, result);
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

    class MyClass : MyBaseClass
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

        await VerifyDiagnostic(original, "Equals() and GetHashcode() must be implemented together on MyClass");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task EqualsAndGetHashcodeNotImplemented_Partial()
    {
        var file1 = @"
partial class MyClass
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }
}";

        var file2 = @"
partial class MyClass
{
    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}";

        await VerifyDiagnostic(new string[] { file1, file2 });
    }
}