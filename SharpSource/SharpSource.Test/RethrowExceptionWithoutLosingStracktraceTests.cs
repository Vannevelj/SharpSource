using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.RethrowExceptionWithoutLosingStacktraceAnalyzer, SharpSource.Diagnostics.RethrowExceptionWithoutLosingStacktraceCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class RethrowExceptionWithoutLosingStracktraceTests
{
    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrowArgument()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception e)
            {
                {|#0:throw e;|}
            }
        }
    }
}";

        var result = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Rethrown exception loses the stacktrace."), result);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_ThrowsANewException()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception e)
            {
                throw new Exception(""test"", e);
            }
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrows()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_ThrowingANewPredefinedException()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception e)
            {
                var newException = new Exception(""test"", e);
                throw newException;
            }
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithThrowStatementOutsideCatchClause()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            throw new Exception();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrowArgument_AndNoIdentifier()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception)
            {
                var e = new Exception();
                throw e;
            }
        }
    }
}";
        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrow_AndNoIdentifier()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            try
            {

            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}";
        await VerifyCS.VerifyNoDiagnostic(original);
    }
}