using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ExceptionThrownFromProhibitedContextTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ExceptionThrownFromProhibitedContextAnalyzer();

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_ImplicitOperator_ToType()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from implicit operator MyClass in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_ImplicitOperator_FromType()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from implicit operator double in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_ImplicitOperator_ExplicitOperator()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_PropertyGetter()
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
			    throw new ArgumentException();
		    }
		    set
		    {
			    
		    }
	    }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_PropertyGetter_Setter()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_PropertyGetter_AutoProperty()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_PropertyGetter_ExpressionBodiedProperty()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_PropertyGetter_NoSetter()
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
			    throw new ArgumentException();
		    }
	    }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_StaticConstructor_Class()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from MyClass its static constructor");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_StaticConstructor_Struct()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from MyStruct its static constructor");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_StaticConstructor_InstanceConstructor()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_FinallyBlock()
    {
        var original = @"
using System;
using System.Text;

try { } finally { throw new ArgumentException(); }
";

        VerifyDiagnostic(original, "An exception is thrown from a finally block");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_FinallyBlock_Nested()
    {
        var original = @"
using System;
using System.Text;

try { } finally { try { } catch { throw new ArgumentException(); } }
";

        VerifyDiagnostic(original, "An exception is thrown from a finally block");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_FinallyBlock_NoException()
    {
        var original = @"
using System;
using System.Text;

try { } finally { }
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_FinallyBlock_NoFinally()
    {
        var original = @"
using System;
using System.Text;

try { } catch { }
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    public void ExceptionThrownFromProhibitedContext_FinallyBlock_AllowedExceptions(string exception)
    {
        var original = $@"
using System;
using System.Text;

try {{ }} finally {{ throw new {exception}(); }}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_EqualityOperator_EqualOperator()
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
		    throw new ArgumentException();
	    }

	    public static bool operator !=(double d, MyClass mc)
	    {
		    return false;
	    }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the == operator between double and MyClass in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_EqualityOperator_NotEqualOperator()
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
		    throw new ArgumentException();
	    }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the != operator between double and MyClass in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_EqualityOperator_NoThrow()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Dispose()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Dispose() method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Dispose_BoolArgument()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Dispose(bool) method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Dispose_NoIDisposable()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Dispose() method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Dispose_DifferentMethod()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Finalize()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the finalizer method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Finalize_NoThrow()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_GetHashCode()
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
            throw new ArgumentException();
        }
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the GetHashCode() method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_GetHashCode_NoThrow()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_GetHashCode_HidingMember()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Equals()
    {
        var original = @"
using System;
using System.Text;

class MyClass
{
	public override bool Equals(object o)
    {
        throw new ArgumentException();
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Equals(object) method in type MyClass");
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    public void ExceptionThrownFromProhibitedContext_Equals_AllowedExceptions(string exception)
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Equals_IEquatable()
    {
        var original = @"
using System;
using System.Text;

public class MyClass : IEquatable<MyClass>
{
	public bool Equals(MyClass o)
    {
        throw new ArgumentException();
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Equals(MyClass) method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Equals_NoThrow()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Equals_HidingMember()
    {
        var original = @"
using System;
using System.Text;

public class MyClass
{
	public bool Equals(object o)
    {
        throw new ArgumentException();
    }
}";

        VerifyDiagnostic(original, "An exception is thrown from the Equals(object) method in type MyClass");
    }

    [TestMethod]
    public void ExceptionThrownFromProhibitedContext_Indexer()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/118")]
    public void ExceptionThrownFromProhibitedContext_EmptyThrow()
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
			    throw;
            }
            return 5;
		}
	}
}";

        VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/117")]
    public void ExceptionThrownFromProhibitedContext_MemberAccessExpression()
    {
        var original = @"
using System;

class MyClass
{
	int MyProp
	{
		get
		{
			throw Exceptions.Unreachable;
		}
	}
}

static class Exceptions
{
	public static Exception Unreachable = new Exception();
}
";

        VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }
}