using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.DivideIntegerByIntegerAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class DivideIntegerByIntegerTests
{
    [TestMethod]
    public async Task DivideIntegerByInteger_TwoIntegers()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                int result = {|#0:5 / 6|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression 5 / 6 are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_IntegerAndDouble()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5 / 6.0;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_DoubleAndInteger()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5.0 / 6;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_DoubleAndDouble()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5.0 / 6.0;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ThreeIntegerOperands()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5 / 6 / 2;
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("The operands in the divisive expression 5 / 6 are both integers and result in an implicit rounding.").WithSpan(8, 33, 8, 38),
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("The operands in the divisive expression 5 / 6 / 2 are both integers and result in an implicit rounding.").WithSpan(8, 33, 8, 42));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ThreeIntegerOperands_TwoSubsequentIntegers_IntegerDivisionEvaluatedFirst()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = {|#0:5 / 6|} / 2.0;
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression 5 / 6 are both integers and result in an implicit rounding."));
    }

    /// <summary>
    /// This scenario is less important because 5.0 / 6 is evaluated first and results in a double.
    /// In practice there will be no integer division here.
    /// </summary>
    [TestMethod]
    public async Task DivideIntegerByInteger_ThreeIntegerOperands_TwoSubsequentIntegers_IntegerDivisionEvaluatedLast()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5.0 / 6 / 2;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ThreeIntegerOperands_NoSubsequentIntegerOperands()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                double result = 5 / 6.0 / 2;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_DynamicOperand()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                dynamic x = 5;
                double result = x / 3;
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_TwoIntegers_OneAsVariable()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                var x = 5;
                double result = {|#0:x / 3|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / 3 are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_TwoIntegers_TwoAsVariables()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                var x = 5;
                var y = 6;
                double result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_TwoIntegers_MethodReference()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                var y = 6;
                double result = {|#0:IntMethod() / y|};
            }

            int IntMethod()
            {
                return 5;
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression IntMethod() / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_TwoIntegers_InsideExpression()
    {
        var original = @"
    using System;

    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                Console.WriteLine({|#0:5 / 6|});
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression 5 / 6 are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ShortAndShort()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                short x = 5;
                short y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_LongAndLong()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                long x = 5;
                long y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_LongAndInt()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                long x = 5;
                int y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ULongAndULong()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                ulong x = 5;
                ulong y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_UIntAndUInt()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                uint x = 5;
                uint y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_UShortAndUShort()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                ushort x = 5;
                ushort y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_ByteAndByte()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                byte x = 5;
                byte y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }

    [TestMethod]
    public async Task DivideIntegerByInteger_SByteAndSByte()
    {
        var original = @"
    namespace ConsoleApplication1
    {
        class MyClass
        {   
            void Method()
            {
                sbyte x = 5;
                sbyte y = 6;
                var result = {|#0:x / y|};
            }
        }
    }";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("The operands in the divisive expression x / y are both integers and result in an implicit rounding."));
    }
}