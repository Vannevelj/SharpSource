using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.RecursiveOperatorOverloadAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class RecursiveEqualityOperatorOverloadTests
{
    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithEqualityOperators()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2)
	    {
		    return a1 {|#0:==|} a2;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return a1 {|#1:!=|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithEquals()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
        public static A operator ==(A a1, A a2)
        {
	        var a = a1.Equals(a2);
		    return a1;
        }

	    public static A operator !=(A a1, A a2)
	    {
		    var a = a1.Equals(a2);
		    return a1;
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDifferentComparison()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
        public static A operator ==(A a1, A a2)
        {
	        var a = 1 == 1;
		    return a1;
        }

	    public static A operator !=(A a1, A a2)
	    {
		    var a = 1 == 1;
		    return a1;
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDifferentOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
        public static A operator ==(A a1, A a2)
        {
	        return a1 + a2;
        }

	    public static A operator !=(A a1, A a2)
	    {
		    return a1 + a2;
	    }

        public static A operator +(A a1, A a2)
	    {
		    return a1;
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNullComparison()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2)
	    {
		    return a1 {|#0:==|} null;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return a1 {|#1:!=|} null;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNullComparisonLeftHand()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2)
	    {
		    return null {|#0:==|} a2;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return null {|#1:!=|} a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithIs()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator ==(A a1, A a2)
	    {
		    return a1 is A;
	    }

	    public static bool operator !=(A a1, A a2)
	    {
		    return !(a1 is A);
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithReturnNull()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2)
	    {
		    return null;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return null;
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithExpressionBody()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2) => a1 {|#0:+|} a2;
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPlusOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2)
	    {
		    return a1 {|#0:+|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithUnaryPlusOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1)
	    {
		    return {|#0:+|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMinusOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator -(A a1, A a2)
	    {
		    return a1 {|#0:-|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithUnaryMinusOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator -(A a1)
	    {
		    return {|#0:-|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMultiplicationOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator *(A a1, A a2)
	    {
		    return a1 {|#0:*|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDivisionOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator /(A a1, A a2)
	    {
		    return a1 {|#0:/|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNotOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator !(A a1)
	    {
		    return {|#0:!|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithBitwiseNotOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ~(A a1)
	    {
		    return {|#0:~|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPostFixIncrementOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ++(A a1)
	    {
		    return a1{|#0:++|};
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPreFixIncrementOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ++(A a1)
	    {
		    return {|#0:++|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDecrementOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator --(A a1)
	    {
		    return {|#0:--|}a1;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator {|#0:true|}(A a1)
	    {
		    if (a1)
			    return true;
		    else
			    return false;
	    }

	    public static bool operator {|#1:false|}(A a1)
	    {
		    if (a1)
			    return false;
		    else
			    return true;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperatorAsExpressionBody()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator {|#0:true|}(A a1) => a1 ? true : false;

	    public static bool operator {|#1:false|}(A a1) => a1 ? false : true;
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperatorAsExpressionBodyWithNestedConditionals()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator {|#0:true|}(A a1) => a1 ? true : a1 ? false : true;

	    public static bool operator {|#1:false|}(A a1) => a1 ? false : a1 ? true : false;
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"),
            VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithLeftShiftOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator <<(A a1, int a2)
	    {
		    return a1 {|#0:<<|} 5;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithRightShiftOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator >>(A a1, int a2)
	    {
		    return a1 {|#0:>>|} 5;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithXorOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ^(A a1, A a2)
	    {
		    return a1 {|#0:^|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithOrOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator |(A a1, A a2)
	    {
		    return a1 {|#0:||} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithAndOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator &(A a1, A a2)
	    {
		    return a1 {|#0:&|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithModOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator %(A a1, A a2)
	    {
		    return a1 {|#0:%|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithGreaterLesserThanEqualityOperators()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator >=(A a1, A a2)
	    {
		    return a1 {|#0:>=|} a2;
	    }

	    public static A operator <=(A a1, A a2)
	    {
		    return a1 {|#1:<=|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithCastOperator()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
        public static implicit operator string(A a)
        {
	        return ""test"";
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMultipleOperators()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2)
	    {
            var a = a1 {|#0:+|} a2;
		    return a {|#1:+|} a2;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(location: 0).WithMessage("Recursively using overloaded operator"), VerifyCS.Diagnostic(location: 1).WithMessage("Recursively using overloaded operator"));
    }
}