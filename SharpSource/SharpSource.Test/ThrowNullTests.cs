using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ThrowNullAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ThrowNullTests
{
    [TestMethod]
    public async Task ThrowNull_ThrowsNullLiteral()
    {
        var original = @"{|#0:throw null;|}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Throwing null will always result in a runtime exception"));
    }

    [TestMethod]
    public async Task ThrowNull_ThrowsNullConstant()
    {
        var original = @"
const System.Exception NullConst = null;
{|#0:throw NullConst;|}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Throwing null will always result in a runtime exception"));
    }

    [TestMethod]
    public async Task ThrowNull_ThrowsNullCast()
    {
        var original = @"{|#0:throw (System.Exception)null;|}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Throwing null will always result in a runtime exception"));
    }

    [TestMethod]
    public async Task ThrowNull_DoesNotThrowNull()
    {
        var original = @"throw new System.Exception();";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ThrowNull_Rethrow()
    {
        var original = @"
try {

} catch (System.Exception) {
    throw;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}