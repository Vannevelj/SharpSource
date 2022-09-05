using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class HttpClientInstantiatedDirectlyAnalyzerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new HttpClientInstantiatedDirectlyAnalyzer();

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_Constructor()
    {
        var original = @"
using System.Net.Http;

var g = new HttpClient();
";

        await VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_FullNameAsync()
    {
        var original = @"
var g = new System.Net.Http.HttpClient();
";

        await VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_HttpClient_SelfDefinedAsync()
    {
        var original = @"
class HttpClient { }

class MyClass
{
    void Method()
    {
        var g = new HttpClient();
    }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_HttpClient_AsUsedAsync()
    {
        var original = @"
using System.Net.Http;

class MyClass
{
    void Method(HttpClient client)
    {
    }
}
";

        await VerifyDiagnostic(original);
    }
}