using System.Threading.Tasks;
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
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrowArgumentAsync()
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

        await VerifyDiagnostic(original, RethrowExceptionWithoutLosingStacktraceAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_ThrowsANewExceptionAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrowsAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_ThrowingANewPredefinedExceptionAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithThrowStatementOutsideCatchClauseAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrowArgument_AndNoIdentifierAsync()
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
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task RethrowExceptionWithoutLosingStracktrace_WithRethrow_AndNoIdentifierAsync()
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
        await VerifyDiagnostic(original);
    }
}