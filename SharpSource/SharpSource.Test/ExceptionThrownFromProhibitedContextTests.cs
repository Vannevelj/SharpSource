using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ExceptionThrownFromProhibitedContextTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ExceptionThrownFromProhibitedContextAnalyzer();

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_ToTypeAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from implicit operator MyClass in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_FromTypeAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from implicit operator double in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_ImplicitOperator_ExplicitOperatorAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetterAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_SetterAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_AutoPropertyAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_ExpressionBodiedPropertyAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_PropertyGetter_NoSetterAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_ClassAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from MyClass its static constructor");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_StructAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from MyStruct its static constructor");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_StaticConstructor_InstanceConstructorAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlockAsync()
    {
        var original = @"
using System;
using System.Text;

try { } finally { throw new ArgumentException(); }
";

        await VerifyDiagnostic(original, "An exception is thrown from a finally block");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_NestedAsync()
    {
        var original = @"
using System;
using System.Text;

try { } finally { try { } catch { throw new ArgumentException(); } }
";

        await VerifyDiagnostic(original, "An exception is thrown from a finally block");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_NoExceptionAsync()
    {
        var original = @"
using System;
using System.Text;

try { } finally { }
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_NoFinallyAsync()
    {
        var original = @"
using System;
using System.Text;

try { } catch { }
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    public async Task ExceptionThrownFromProhibitedContext_FinallyBlock_AllowedExceptionsAsync(string exception)
    {
        var original = $@"
using System;
using System.Text;

try {{ }} finally {{ throw new {exception}(); }}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_EqualOperatorAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the == operator between double and MyClass in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_NotEqualOperatorAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the != operator between double and MyClass in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualityOperator_NoThrowAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_DisposeAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Dispose() method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_BoolArgumentAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Dispose(bool) method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_NoIDisposableAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Dispose() method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Dispose_DifferentMethodAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_FinalizeAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the finalizer method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Finalize_NoThrowAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCodeAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the GetHashCode() method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCode_NoThrowAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_GetHashCode_HidingMemberAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_EqualsAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Equals(object) method in type MyClass");
    }

    [TestMethod]
    [DataRow("NotImplementedException")]
    [DataRow("NotSupportedException")]
    [DataRow("System.NotImplementedException")]
    [DataRow("System.NotSupportedException")]
    public async Task ExceptionThrownFromProhibitedContext_Equals_AllowedExceptionsAsync(string exception)
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_IEquatableAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Equals(MyClass) method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_NoThrowAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_Equals_HidingMemberAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the Equals(object) method in type MyClass");
    }

    [TestMethod]
    public async Task ExceptionThrownFromProhibitedContext_IndexerAsync()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/118")]
    public async Task ExceptionThrownFromProhibitedContext_EmptyThrowAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/117")]
    public async Task ExceptionThrownFromProhibitedContext_MemberAccessExpressionAsync()
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

        await VerifyDiagnostic(original, "An exception is thrown from the getter of property MyProp");
    }
}