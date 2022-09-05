using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class RecursiveEqualityOperatorOverloadTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new RecursiveOperatorOverloadAnalyzer();

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithEqualityOperatorsAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2) 
	    {
		    return a1 == a2;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return a1 != a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithEqualsAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDifferentComparisonAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDifferentOperatorAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNullComparisonAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2) 
	    {
		    return a1 == null;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return a1 != null;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNullComparisonLeftHandAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ==(A a1, A a2) 
	    {
		    return null == a2;
	    }

	    public static A operator !=(A a1, A a2)
	    {
		    return null != a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithIsAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithReturnNullAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithExpressionBodyAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2) => a1 + a2;
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPlusOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2)
	    {
		    return a1 + a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithUnaryPlusOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1)
	    {
		    return +a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMinusOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator -(A a1, A a2)
	    {
		    return a1 - a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithUnaryMinusOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator -(A a1)
	    {
		    return -a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMultiplicationOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator *(A a1, A a2)
	    {
		    return a1 * a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDivisionOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator /(A a1, A a2)
	    {
		    return a1 / a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithNotOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator !(A a1)
	    {
		    return !a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithBitwiseNotOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ~(A a1)
	    {
		    return ~a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPostFixIncrementOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ++(A a1)
	    {
		    return a1++;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithPreFixIncrementOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ++(A a1)
	    {
		    return ++a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithDecrementOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator --(A a1)
	    {
		    return --a1;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator true(A a1)
	    {
		    if (a1)
			    return true;
		    else
			    return false;
	    }

	    public static bool operator false(A a1)
	    {
		    if (a1)
			    return false;
		    else
			    return true;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperatorAsExpressionBodyAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator true(A a1) => a1 ? true : false;

	    public static bool operator false(A a1) => a1 ? false : true;
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithTrueAndFalseOperatorAsExpressionBodyWithNestedConditionalsAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static bool operator true(A a1) => a1 ? true : a1 ? false : true;

	    public static bool operator false(A a1) => a1 ? false : a1 ? true : false;
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator", "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithLeftShiftOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator <<(A a1, int a2)
	    {
		    return a1 << 5;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithRightShiftOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator >>(A a1, int a2)
	    {
		    return a1 >> 5;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithXorOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator ^(A a1, A a2)
	    {
		    return a1 ^ a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithOrOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator |(A a1, A a2)
	    {
		    return a1 | a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithAndOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator &(A a1, A a2)
	    {
		    return a1 & a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithModOperatorAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator %(A a1, A a2)
	    {
		    return a1 % a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithGreaterLesserThanEqualityOperatorsAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator >=(A a1, A a2) 
	    {
		    return a1 >= a2;
	    }

	    public static A operator <=(A a1, A a2)
	    {
		    return a1 <= a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithCastOperatorAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RecursiveEqualityOperatorOverload_WithMultipleOperatorsAsync()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class A
    {
	    public static A operator +(A a1, A a2)
	    {
            var a = a1 + a2;
		    return a + a2;
	    }
    }
}";

        await VerifyDiagnostic(original, "Recursively using overloaded operator", "Recursively using overloaded operator");
    }
}