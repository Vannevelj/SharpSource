using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.HttpClientInstantiatedDirectlyAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class HttpClientInstantiatedDirectlyAnalyzerTests
{
    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_Constructor()
    {
        var original = @"
using System.Net.Http;

var g = {|#0:new HttpClient()|};";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("HttpClient was instantiated directly. Use IHttpClientFactory instead"));
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_ImplicitConstructor()
    {
        var original = @"
using System.Net.Http;

HttpClient g = {|#0:new()|};";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("HttpClient was instantiated directly. Use IHttpClientFactory instead"));
    }

    [TestMethod]
    public async Task HttpClientInstantiatedDirectly_FullName()
    {
        var original = @"
var g = {|#0:new System.Net.Http.HttpClient()|};";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("HttpClient was instantiated directly. Use IHttpClientFactory instead"));
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
}";

        await VerifyCS.VerifyNoDiagnostic(original);
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
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}