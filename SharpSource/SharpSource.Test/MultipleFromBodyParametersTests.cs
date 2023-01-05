using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.MultipleFromBodyParametersAnalyzer>;

namespace SharpSource.Test;

[TestClass]
public class MultipleFromBodyParametersTests
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new MultipleFromBodyParametersAnalyzer();

    [TestMethod]
    [DataRow("[FromBody]")]
    [DataRow("[FromBodyAttribute]")]
    [DataRow("[Microsoft.AspNetCore.Mvc.FromBody]")]
    public async Task MultipleFromBodyParameters_MultipleAsync(string attribute)
    {
        var original = $@"
using Microsoft.AspNetCore.Mvc;

class MyController
{{
    void DoThing({attribute} string first, {attribute} string second) {{ }}
}}
";

        await VerifyDiagnostic(original, "Method DoThing specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.");
    }

    [TestMethod]
    [Ignore("Minimal Web API is not supported yet. See https://github.com/Vannevelj/SharpSource/issues/140")]
    [DataRow("[FromBody]")]
    [DataRow("[FromBodyAttribute]")]
    [DataRow("[Microsoft.AspNetCore.Mvc.FromBody]")]
    public async Task MultipleFromBodyParameters_MinimalWebApiAsync(string attribute)
    {
        var original = $@"
using Microsoft.AspNetCore.Mvc;

var app = new WebApplication();
app.MapGet(""/"", ({attribute} string first, {attribute} string second, Service service) => {{ }});

class WebApplication {{
    public void MapGet(string path, System.Action<string, string, Service> handler) {{ }}
}}
class Service {{ }}";

        await VerifyDiagnostic(original, "Method specifies multiple [FromBody] parameters but only one is allowed. Specify a wrapper type or use [FromForm], [FromRoute], [FromHeader] and [FromQuery] instead.");
    }

    [TestMethod]
    public async Task MultipleFromBodyParameters_Single()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromBody] string first, string second) { }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task MultipleFromBodyParameters_MultipleDifferent()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromBody] string first, [FromQuery] string second) { }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task MultipleFromBodyParameters_Different()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing([FromQuery] string first, [FromRoute] string second) { }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task MultipleFromBodyParameters_NoAttributes()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing(string first, string second) { }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task MultipleFromBodyParameters_NoParameters()
    {
        var original = @"
using Microsoft.AspNetCore.Mvc;

class MyController
{
    void DoThing() { }
}
";

        await VerifyDiagnostic(original);
    }
}