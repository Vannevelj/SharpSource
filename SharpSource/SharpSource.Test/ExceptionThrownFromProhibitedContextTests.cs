using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ExceptionThrownFromProhibitedContextAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ExceptionThrownFromProhibitedContextTests
{
    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_ToType()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
        public static implicit operator MyClass(double d)
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.ImplicitOperatorRule).WithMessage("An exception is thrown from implicit operator MyClass in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_FromType()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
        public static implicit operator double(MyClass d)
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.ImplicitOperatorRule).WithMessage("An exception is thrown from implicit operator double in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_ExplicitOperator()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
        public static explicit operator double(MyClass d)
        {
            throw new ArgumentException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public int MyProp
	    {
		    get
		    {
			    {|#0:throw new ArgumentException();|}
		    }
		    set
		    {

		    }
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.PropertyGetterRule).WithMessage("An exception is thrown from the getter of property MyProp"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_Setter()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public int MyProp
	    {
		    get
		    {
			    return 5;
		    }
		    set
		    {
			    throw new ArgumentException();
		    }
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_AutoProperty()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public int MyProp { get; set; }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_ExpressionBodiedProperty()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public int MyProp => 5;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_NoSetter()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public int MyProp
	    {
		    get
		    {
			    {|#0:throw new ArgumentException();|}
		    }
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.PropertyGetterRule).WithMessage("An exception is thrown from the getter of property MyProp"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_Class()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    static MyClass()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.StaticConstructorRule).WithMessage("An exception is thrown from MyClass its static constructor"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_Struct()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyStruct
    {
	    static MyStruct()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.StaticConstructorRule).WithMessage("An exception is thrown from MyStruct its static constructor"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_InstanceConstructor()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    MyClass()
        {
            throw new ArgumentException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock()
    {
        var original = @"
using System;
using System.Text;

try { } finally { {|#0:throw new ArgumentException();|} }
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.FinallyBlockRule).WithMessage("An exception is thrown from a finally block"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_Nested()
    {
        var original = @"
using System;
using System.Text;

try { } finally { try { } catch { {|#0:throw new ArgumentException();|} } }
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.FinallyBlockRule).WithMessage("An exception is thrown from a finally block"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_NoException()
    {
        var original = @"
using System;
using System.Text;

try { } finally { }
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_NoFinally()
    {
        var original = @"
using System;
using System.Text;

try { } catch { }
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("PlatformNotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    [DataRow("System.PlatformNotSupportedException")]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_AllowedExceptions(string exception)
    {
        var original = $@"
using System;
using System.Text;

try {{ }} finally {{ throw new {exception}(); }}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_EqualOperator()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public static bool operator ==(double d, MyClass mc)
	    {
		    {|#0:throw new ArgumentException();|}
	    }

	    public static bool operator !=(double d, MyClass mc)
	    {
		    return false;
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.EqualityOperatorRule).WithMessage("An exception is thrown from the == operator between double and MyClass in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_NotEqualOperator()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public static bool operator ==(double d, MyClass mc)
	    {
		    return false;
	    }

	    public static bool operator !=(double d, MyClass mc)
	    {
		    {|#0:throw new ArgumentException();|}
	    }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.EqualityOperatorRule).WithMessage("An exception is thrown from the != operator between double and MyClass in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_NoThrow()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public static bool operator ==(double d, MyClass mc)
	    {
		    return false;
	    }

	    public static bool operator !=(double d, MyClass mc)
	    {
		    return false;
	    }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass : IDisposable
    {
	    public void Dispose()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.DisposeRule).WithMessage("An exception is thrown from the Dispose() method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_BoolArgument()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
        }

	    public void Dispose(bool dispose)
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.DisposeRule).WithMessage("An exception is thrown from the Dispose(bool) method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_NoIDisposable()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public void Dispose()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.DisposeRule).WithMessage("An exception is thrown from the Dispose() method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_DifferentMethod()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass : IDisposable
    {
	    public void OtherName()
        {
            throw new ArgumentException();
        }

        public void Dispose()
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Finalize()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    ~MyClass()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.FinalizerRule).WithMessage("An exception is thrown from the finalizer method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Finalize_NoThrow()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    ~MyClass()
        {

        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCode()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public override int GetHashCode()
        {
            {|#0:throw new ArgumentException();|}
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.GetHashCodeRule).WithMessage("An exception is thrown from the GetHashCode() method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCode_NoThrow()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    public class MyClass
    {
	    public override int GetHashCode()
        {
            return 5;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCode_HidingMember()
    {
        var original = @"
using System;
using System.Text;

class MyClass
{
	public int GetHashCode()
    {
        throw new ArgumentException();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals()
    {
        var original = @"
using System;
using System.Text;

class MyClass
{
	public override bool Equals(object o)
    {
        {|#0:throw new ArgumentException();|}
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.EqualsRule).WithMessage("An exception is thrown from the Equals(object) method in type MyClass"));
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("PlatformNotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    [DataRow("System.PlatformNotSupportedException")]
    public async Task ExceptionThrownFromProhibitedContext_Equals_AllowedExceptions(string exception)
    {
        var original = $@"
using System;
using System.Text;

class MyClass
{{
	public override bool Equals(object o)
    {{
        throw new {exception}();
    }}
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_IEquatable()
    {
        var original = @"
using System;
using System.Text;

public class MyClass : IEquatable<MyClass>
{
	public bool Equals(MyClass o)
    {
        {|#0:throw new ArgumentException();|}
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.EqualsRule).WithMessage("An exception is thrown from the Equals(MyClass) method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_NoThrow()
    {
        var original = @"
using System;
using System.Text;

public class MyClass
{
	public override bool Equals(object o)
    {
        return false;
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_HidingMember()
    {
        var original = @"
using System;
using System.Text;

public class MyClass
{
	public bool Equals(object o)
    {
        {|#0:throw new ArgumentException();|}
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.EqualsRule).WithMessage("An exception is thrown from the Equals(object) method in type MyClass"));
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Indexer()
    {
        var original = @"
using System;

class MyClass
{
    public string this[int i]
    {
	    get
	    {
		    throw new ArgumentException();
	    }

	    set { }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/118")]
    public async Task ExceptionThrownFromProhibitedContext_EmptyThrow()
    {
        var original = @"
using System;

class MyClass
{
	int MyProp
	{
		get
		{
            try { }
            catch {
			    {|#0:throw;|}
            }
            return 5;
		}
	}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.PropertyGetterRule).WithMessage("An exception is thrown from the getter of property MyProp"));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/117")]
    public async Task ExceptionThrownFromProhibitedContext_MemberAccessExpression()
    {
        var original = @"
using System;

class MyClass
{
	int MyProp
	{
		get
		{
			{|#0:throw Exceptions.Unreachable;|}
		}
	}
}

static class Exceptions
{
	public static Exception Unreachable = new Exception();
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(ExceptionThrownFromProhibitedContextAnalyzer.PropertyGetterRule).WithMessage("An exception is thrown from the getter of property MyProp"));
    }
}