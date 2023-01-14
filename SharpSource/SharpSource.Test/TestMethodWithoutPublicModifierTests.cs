using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.TestMethodWithoutPublicModifierAnalyzer, SharpSource.Diagnostics.TestMethodWithoutPublicModifierCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class TestMethodWithoutPublicModifierTests
{
    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndTestAttribute()
    {
        var original = @"
    using System;
    using NUnit.Framework;

    namespace ConsoleApplication1
    {
        [TestFixture]
        public class MyClass
        {
            [Test]
            public void Method()
            {

            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndTestMethodAttribute()
    {
        var original = @"
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace ConsoleApplication1
    {
        [TestClass]
        public class MyClass
        {
            [TestMethod]
            public void Method()
            {

            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndFactAttribute()
    {
        var original = @"
    using System;
    using Xunit;

    namespace ConsoleApplication1
    {
        public class MyClass
        {
            [Fact]
            public void Method()
            {

            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndTestAttribute()
    {
        var original = @"
using System;
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    public class MyClass
    {
        [Test]
        internal void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    public class MyClass
    {
        [Test]
        public void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndTestMethodAttribute()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        internal void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        public void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndFactAttribute()
    {
        var original = @"
using System;
using Xunit;

namespace ConsoleApplication1
{
    public class MyClass
    {
        [Fact]
        internal void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using Xunit;

namespace ConsoleApplication1
{
    public class MyClass
    {
        [Fact]
        public void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndMultipleAttributes()
    {
        var original = @"
using System;
using Xunit;

public class MyClass
{
    [Obsolete]
    [Fact]
    public void Method()
    {

    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithProtectedInternalModifierAndTestMethodAttribute()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        protected internal virtual void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        public virtual void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithMultipleModifiersAndTestMethodAttribute()
    {
        var original = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        internal virtual void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleApplication1
{
    [TestClass]
    public class MyClass
    {
        [TestMethod]
        public virtual void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithoutModifierAndTestAttribute()
    {
        var original = @"
using System;
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    public class MyClass
    {
        [Test]
        void {|#0:Method|}()
        {

        }
    }
}";

        var result = @"
using System;
using NUnit.Framework;

namespace ConsoleApplication1
{
    [TestFixture]
    public class MyClass
    {
        [Test]
        public void Method()
        {

        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithoutTestAttribute()
    {
        var original = @"
    using System;

    namespace ConsoleApplication1
    {
        public class MyClass
        {
            private static void Method()
            {

            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/236")]
    public async Task TestMethodWithoutPublicModifier_InheritedAttribute_SingleLevel()
    {
        var original = @"
using System;
using NUnit.Framework;

class MyOwnAttribute : TestAttribute {}

[TestFixture]
public class MyClass
{
    [MyOwnAttribute]
    void {|#0:Method|}()
    {

    }
}";

        var result = @"
using System;
using NUnit.Framework;

class MyOwnAttribute : TestAttribute {}

[TestFixture]
public class MyClass
{
    [MyOwnAttribute]
    public void Method()
    {

    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/236")]
    public async Task TestMethodWithoutPublicModifier_InheritedAttribute_MultipleLevels()
    {
        var original = @"
using System;
using NUnit.Framework;

class MyOwnAttribute : TestAttribute {}
class MySecondAttribute : MyOwnAttribute {}
class MyThirdAttribute : MySecondAttribute {}

[TestFixture]
public class MyClass
{
    [MyThird]
    void {|#0:Method|}()
    {

    }
}";

        var result = @"
using System;
using NUnit.Framework;

class MyOwnAttribute : TestAttribute {}
class MySecondAttribute : MyOwnAttribute {}
class MyThirdAttribute : MySecondAttribute {}

[TestFixture]
public class MyClass
{
    [MyThird]
    public void Method()
    {

    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Test method \"Method\" is not public."), result);
    }
}