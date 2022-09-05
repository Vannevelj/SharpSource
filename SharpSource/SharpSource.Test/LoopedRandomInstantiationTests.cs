using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class LoopedRandomInstantiationTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new LoopedRandomInstantiationAnalyzer();

    [TestMethod]
    public async Task LoopedRandomInstantiation_WhileLoopAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            while (true)
            {
                var rand = new Random();
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_DoWhileLoopAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            do
            {
                var rand = new Random();
            } while (true);
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_ForLoopAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            for (var i = 0; i > 5; i++)
            {
                var rand = new Random(4);
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_ForeachLoopAsync()
    {
        var original = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<string>();
            foreach (var item in list)
            {
                var rand = new Random();
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_MultipleDeclaratorsInDeclarationAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            while (true)
            {
                Random rand = new Random(), rind = new Random(2);
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"),
                                   string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rind"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_MultipleLevelsOfNestingAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            while (true)
            {
                if (true)
                {
                    Random rand = new Random();
                }
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_RandomInstanceNotInLoopAsync()
    {
        var original = @"
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var rand = new Random();
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_RandomNotSystemRandomAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    class Random {}

    class MyClass
    {
        void Method()
        {
            var rand = new Random();
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_TypeIsObject_DoesNotCrashAnalyzerBecauseContainingNamespaceIsNullAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            object[] o = {};
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_StructAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    struct MyStruct
    {
        void Method()
        {
            while (true)
            {
                var rand = new Random();
            }
        }
    }
}";

        await VerifyDiagnostic(original, string.Format(LoopedRandomInstantiationAnalyzer.Rule.MessageFormat.ToString(), "rand"));
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_NotInLoop_StructAsync()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    struct MyStruct
    {
        void Method()
        {
            var rand = new Random();
        }
    }
}";

        await VerifyDiagnostic(original);
    }
}