using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class ThrowNullTests : CSharpDiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ThrowNullAnalyzer();

    [TestMethod]
    public void ThrowNull_ThrowsNull()
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
                throw null;
            }
        }
    }";

        VerifyDiagnostic(original, "Throwing null will always result in a runtime exception");
    }

    [TestMethod]
    public void ThrowNull_DoesNotThrowNull()
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
    public void ThrowNull_Rethrow()
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
                try {

                } catch (Exception) {
                    throw;
                }
            }
        }
    }";

        VerifyDiagnostic(original);
    }
}