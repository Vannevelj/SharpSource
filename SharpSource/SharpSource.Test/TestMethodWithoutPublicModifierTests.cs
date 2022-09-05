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
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndTestMethodAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndFactAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndTestAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndTestMethodAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithInternalModifierAndFactAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithPublicModifierAndMultipleAttributesAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithProtectedInternalModifierAndTestMethodAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithMultipleModifiersAndTestMethodAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithoutModifierAndTestAttributeAsync()
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
    public async Task TestMethodWithoutPublicModifier_WithoutTestAttributeAsync()
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