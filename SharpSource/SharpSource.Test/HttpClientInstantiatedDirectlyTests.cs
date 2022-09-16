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
    public async Task HttpClientInstantiatedDirectly_ImplicitConstructor()
    {
        var original = @"
using System.Net.Http;

HttpClient g = new();
";

        await VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_FullName()
    {
        var original = @"
var g = new System.Net.Http.HttpClient();
";

        await VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_HttpClient_SelfDefined()
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
    public async Task HttpClientInstantiatedDirectly_HttpClient_AsUsed()
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