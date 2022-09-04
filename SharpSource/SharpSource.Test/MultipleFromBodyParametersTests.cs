using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class MultipleFromBodyParametersTests : CSharpDiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new MultipleFromBodyParametersAnalyzer();

    [TestMethod]
    [DataRow("[FromBody]")]
    [DataRow("[FromBodyAttribute]")]
    [DataRow("[Microsoft.AspNetCore.Mvc.FromBody]")]
    public void MultipleFromBodyParameters_Multiple(string attribute)
    {
        var original = $@"
using Microsoft.AspNetCore.Mvc;

class MyController
{{
    void DoThing({attribute} string first, {attribute} string second) {{ }}
}}
";

        VerifyDiagnostic(original, "Method DoThing specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.");
    }

    [TestMethod]
    [Ignore("Minimal Web API is not supported yet")]
    [DataRow("[FromBody]")]
    [DataRow("[FromBodyAttribute]")]
    [DataRow("[Microsoft.AspNetCore.Mvc.FromBody]")]
    public void MultipleFromBodyParameters_MinimalWebApi(string attribute)
    {
        var original = $@"
using Microsoft.AspNetCore.Mvc;

var app = new WebApplication();
app.MapGet(""/"", ({attribute} string first, {attribute} string second, Service service) => {{ }});

class WebApplication {{
    public void MapGet(string path, System.Action<string, string, Service> handler) {{ }}
}}
class Service {{ }}";

        VerifyDiagnostic(original, "Method specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.");
    }

    [TestMethod]
    public void MultipleFromBodyParameters_Single()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromBody] string first, string second) { }
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void MultipleFromBodyParameters_MultipleDifferent()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromBody] string first, [FromQuery] string second) { }
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void MultipleFromBodyParameters_Different()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromQuery] string first, [FromRoute] string second) { }
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void MultipleFromBodyParameters_NoAttributes()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing(string first, string second) { }
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void MultipleFromBodyParameters_NoParameters()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing() { }
}
";

        VerifyDiagnostic(original);
    }
}