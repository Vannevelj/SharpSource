using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LoopedRandomInstantiationAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class LoopedRandomInstantiationTests
{
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
                var {|#0:rand = new Random()|};
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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
                var {|#0:rand = new Random()|};
            } while (true);
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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
                var {|#0:rand = new Random(4)|};
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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
                var {|#0:rand = new Random()|};
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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
                Random {|#0:rand = new Random()|}, {|#1:rind = new Random(2)|};
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, new[] {
            VerifyCS.Diagnostic(location: 0).WithMessage("Variable rand of type System.Random is instantiated in a loop."),
            VerifyCS.Diagnostic(location: 1).WithMessage("Variable rind of type System.Random is instantiated in a loop.")
        });
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
                    Random {|#0:rand = new Random()|};
                }
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
                var {|#0:rand = new Random()|};
            }
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Variable rand of type System.Random is instantiated in a loop."));
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}