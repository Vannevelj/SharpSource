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
    public async Task LoopedRandomInstantiation_WhileLoop()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_DoWhileLoop()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_ForLoop()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_ForeachLoop()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_MultipleDeclaratorsInDeclaration()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.", "Variable rind of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_MultipleLevelsOfNesting()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_RandomInstanceNotInLoop()
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
    public async Task LoopedRandomInstantiation_RandomNotSystemRandom()
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
    public async Task LoopedRandomInstantiation_TypeIsObject_DoesNotCrashAnalyzerBecauseContainingNamespaceIsNull()
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
    public async Task LoopedRandomInstantiation_Struct()
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

        await VerifyDiagnostic(original, "Variable rand of type System.Random is instantiated in a loop.");
    }

    [TestMethod]
    public async Task LoopedRandomInstantiation_NotInLoop_Struct()
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