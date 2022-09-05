using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class HttpContextStoredInFieldAnalyzerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new HttpContextStoredInFieldAnalyzer();

    [TestMethod]
    public void HttpContextStoredInField_InField()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext _context;    
}
";

        VerifyDiagnostic(original, "HttpContext was stored in a field. Use IHttpContextAccessor instead");
    }

    [TestMethod]
    public void HttpContextStoredInField_CustomHttpContextClass()
    {
        var original = @"
class HttpContext { }

class Test
{
    private HttpContext _context;    
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void HttpContextStoredInField_InProperty()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext Context { get; set; } 
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void HttpContextStoredInField_AsVariable()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void HttpContextStoredInField_InField_MultipleDeclarators()
    {
        var original = @"
using Microsoft.AspNetCore.Http;

class Test
{
    private HttpContext _context, _context2;    
}
";

        VerifyDiagnostic(original, "HttpContext was stored in a field. Use IHttpContextAccessor instead");
    }
}