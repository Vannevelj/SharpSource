using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.HttpContextStoredInFieldAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class HttpContextStoredInFieldTests
{
    [TestMethod]
    public async Task HttpContextStoredInField_InField()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext {|#0:_context|};
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("HttpContext was stored in a field. Use IHttpContextAccessor instead"));
    }

    [TestMethod]
    public async Task HttpContextStoredInField_CustomHttpContextClass()
    {
        var original = @"
class HttpContext { }

class Test
{
    private HttpContext _context;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_InProperty()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext Context { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_AsVariable()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    Test()
    {
        HttpContext context;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_InField_MultipleDeclarators()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext {|#0:_context|}, {|#1:_context2|};
}";
        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("HttpContext was stored in a field. Use IHttpContextAccessor instead"),
            VerifyCS.Diagnostic(location: 1).WithMessage("HttpContext was stored in a field. Use IHttpContextAccessor instead"));
    }
}