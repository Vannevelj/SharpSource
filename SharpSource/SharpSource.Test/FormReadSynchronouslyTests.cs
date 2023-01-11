using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.FormReadSynchronouslyAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class FormReadSynchronouslyTests
{
    [TestMethod]
    public async Task FormReadSynchronously()
    {
        var original = @"
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

class MyController : Controller
{
    public IActionResult Post()
    {
        var form = {|#0:HttpContext.Request.Form|};
        return Ok();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously accessed HttpRequest.Form. Use HttpRequest.ReadFormAsync() instead"));
    }

    [TestMethod]
    public async Task FormReadSynchronously_WrongType()
    {
        var original = @"
var form = new HttpRequest().Form;

class HttpRequest
{
    public string Form { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task FormReadSynchronously_Chained()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController : Controller
{
    public IActionResult Post()
    {
        var form = {|#0:HttpContext.Request.Form|}.Count;
        return Ok();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Synchronously accessed HttpRequest.Form. Use HttpRequest.ReadFormAsync() instead"));
    }

    [TestMethod]
    public async Task FormReadSynchronously_Nameof()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController : Controller
{
    public IActionResult Post()
    {
        var form = nameof(HttpContext.Request.Form);
        return Ok();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}