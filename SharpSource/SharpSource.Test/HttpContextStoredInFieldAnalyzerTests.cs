using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class HttpContextStoredInFieldAnalyzerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new HttpContextStoredInFieldAnalyzer();

    [TestMethod]
    public async Task HttpContextStoredInField_InFieldAsync()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext _context;    
}
";

        await VerifyDiagnostic(original, "HttpContext was stored in a field. Use IHttpContextAccessor instead");
    }

    [TestMethod]
    public async Task HttpContextStoredInField_CustomHttpContextClassAsync()
    {
        var original = @"
class HttpContext { }

class Test
{
    private HttpContext _context;    
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_InPropertyAsync()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext Context { get; set; } 
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_AsVariableAsync()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    Test()
    {
        HttpContext context;
    }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpContextStoredInField_InField_MultipleDeclaratorsAsync()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext _context, _context2;    
}
";

        await VerifyDiagnostic(original, "HttpContext was stored in a field. Use IHttpContextAccessor instead");
    }
}