using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class TestMethodWithoutPublicModifierTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new TestMethodWithoutPublicModifierAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new TestMethodWithoutPublicModifierCodeFix();

    [TestMethod]
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndTestAttribute()
    {
        var test = @"
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

        await VerifyDiagnostic(test);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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
        internal void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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
        internal void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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
        internal void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
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
        protected internal virtual void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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
        internal virtual void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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
        void Method()
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

        await VerifyDiagnostic(original, string.Format(TestMethodWithoutPublicModifierAnalyzer.Rule.MessageFormat.ToString(), "Method"));
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
    }
}