using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class RethrowExceptionWithoutLosingStracktraceTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new RethrowExceptionWithoutLosingStacktraceAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new RethrowExceptionWithoutLosingStacktraceCodeFix();

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_WithRethrowArgument()
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
                throw e;
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

        VerifyDiagnostic(original, RethrowExceptionWithoutLosingStacktraceAnalyzer.Rule.MessageFormat.ToString());
        VerifyFix(original, result, allowedNewCompilerDiagnosticsId: "CS0168"); // Removing the argument will remove all usages of the e parameter. This will cause a CS0168 warning.
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_ThrowsANewException()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_WithRethrows()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_ThrowingANewPredefinedException()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_WithThrowStatementOutsideCatchClause()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_WithRethrowArgument_AndNoIdentifier()
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
        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void RethrowExceptionWithoutLosingStracktrace_WithRethrow_AndNoIdentifier()
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
        VerifyDiagnostic(original);
    }
}