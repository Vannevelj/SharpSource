using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class HttpClientInstantiatedDirectlyAnalyzerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new HttpClientInstantiatedDirectlyAnalyzer();

    [TestMethod]
    public void HttpClientInstantiatedDirectly_Constructor()
    {
        var original = @"
using System.Net.Http;

var g = new HttpClient();
";

        VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public void HttpClientInstantiatedDirectly_FullName()
    {
        var original = @"
var g = new System.Net.Http.HttpClient();
";

        VerifyDiagnostic(original, "HttpClient was instantiated directly. Use IHttpClientFactory instead");
    }

    [TestMethod]
    public void HttpClientInstantiatedDirectly_HttpClient_SelfDefined()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void HttpClientInstantiatedDirectly_HttpClient_AsUsed()
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

        VerifyDiagnostic(original);
    }
}