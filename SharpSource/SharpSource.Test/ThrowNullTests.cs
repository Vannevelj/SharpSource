using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ThrowNullTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ThrowNullAnalyzer();

    [TestMethod]
    public async Task ThrowNull_ThrowsNullAsync()
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

        await VerifyDiagnostic(original, "Throwing null will always result in a runtime exception");
    }

    [TestMethod]
    public async Task ThrowNull_DoesNotThrowNullAsync()
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
    public async Task ThrowNull_RethrowAsync()
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

        await VerifyDiagnostic(original);
    }
}