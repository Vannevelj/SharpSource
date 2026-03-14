using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.RedisResponseNotHandledAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class RedisResponseNotHandledTests
{
    // ========== Elastic.Clients.Elasticsearch (v8+) tests ==========

    [TestMethod]
    public async Task RedisResponseNotHandled_AsyncCallDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await {|#0:client.SearchAsync<object>()|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_SyncCallDiscarded()
    {
        var original = @"
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        {|#0:client.Search<object>()|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Search was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponseAssignedToVariable()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        var response = await client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponseUsedInCondition()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        var response = await client.SearchAsync<object>();
        if (response.IsValidResponse) { }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponseReturnedFromMethod()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task<SearchResponse<object>> Method()
    {
        var client = new ElasticsearchClient();
        return await client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponsePassedAsArgument()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        Handle(await client.SearchAsync<object>());
    }

    void Handle(TransportResponse response) { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_IndexAsyncDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await {|#0:client.IndexAsync(new object())|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of IndexAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_DeleteAsyncDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await {|#0:client.DeleteAsync(""index"", ""id"")|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of DeleteAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_BulkAsyncDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await {|#0:client.BulkAsync()|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of BulkAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TaskNotAwaited()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        {|#0:client.SearchAsync<object>()|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_MultipleDiscardedCalls()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await {|#0:client.SearchAsync<object>()|};
        await {|#1:client.IndexAsync(new object())|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            [
                VerifyCS.Diagnostic(location: 0).WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"),
                VerifyCS.Diagnostic(location: 1).WithMessage("The response of IndexAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors")
            ]);
    }

    // ========== NEST (v7) tests ==========

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_AsyncCallDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        var client = new ElasticClient();
        await {|#0:client.SearchAsync<object>(s => s)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_SyncCallDiscarded()
    {
        var original = @"
using Nest;

class Test
{
    void Method()
    {
        var client = new ElasticClient();
        {|#0:client.Search<object>(s => s)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Search was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_ResponseAssigned()
    {
        var original = @"
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        var client = new ElasticClient();
        var response = await client.SearchAsync<object>(s => s);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    // ========== OpenSearch tests ==========

    [TestMethod]
    public async Task RedisResponseNotHandled_OpenSearch_AsyncCallDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using OpenSearch.Client;

class Test
{
    async Task Method()
    {
        var client = new OpenSearchClient();
        await {|#0:client.SearchAsync<object>(s => s)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_OpenSearch_SyncCallDiscarded()
    {
        var original = @"
using OpenSearch.Client;

class Test
{
    void Method()
    {
        var client = new OpenSearchClient();
        {|#0:client.Search<object>(s => s)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Search was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_OpenSearch_ResponseAssigned()
    {
        var original = @"
using System.Threading.Tasks;
using OpenSearch.Client;

class Test
{
    async Task Method()
    {
        var client = new OpenSearchClient();
        var response = await client.SearchAsync<object>(s => s);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    // ========== Edge cases ==========

    [TestMethod]
    public async Task RedisResponseNotHandled_NoElasticSearchTypes()
    {
        var original = @"
class Test
{
    void Method()
    {
        var x = 5;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_UnrelatedMethodCallDiscarded()
    {
        var original = @"
using System.Collections.Generic;

class Test
{
    void Method()
    {
        var list = new List<int>();
        list.Add(5);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_HelperMethodReturningResponseDiscarded()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Transport;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        await {|#0:DoSearch()|};
    }

    async Task<SearchResponse<object>> DoSearch()
    {
        var client = new ElasticsearchClient();
        return await client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of DoSearch was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponseAssignedToField()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    private SearchResponse<object> _response;

    async Task Method()
    {
        var client = new ElasticsearchClient();
        _response = await client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_SyncIndexCallDiscarded()
    {
        var original = @"
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        {|#0:client.Index(new object())|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Index was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_UsedInTernary()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task<bool> Method()
    {
        var client = new ElasticsearchClient();
        var response = await client.SearchAsync<object>();
        return response.IsValidResponse ? true : false;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TaskStoredButNotAwaited()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        var task = client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_ResponseUsedInExpressionBody()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    Task<SearchResponse<object>> Method()
    {
        var client = new ElasticsearchClient();
        return client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_DiscardedWithExplicitDiscard()
    {
        var original = @"
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        _ = client.Search<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_SyncDeleteDiscarded()
    {
        var original = @"
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        var client = new ElasticsearchClient();
        {|#0:client.Delete(""index"", ""id"")|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Delete was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    // ========== Try-catch tests ==========

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithoutErrorCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await {|#0:client.SearchAsync<object>()|};
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithIsValidResponseCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await client.SearchAsync<object>();
            if (response.IsValidResponse) { }
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithDebugInformationCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await client.SearchAsync<object>();
            var info = response.DebugInformation;
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithApiCallDetailsCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await client.SearchAsync<object>();
            var details = response.ApiCallDetails;
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithElasticsearchServerErrorCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await client.SearchAsync<object>();
            var err = response.ElasticsearchServerError;
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithTryGetOriginalExceptionCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await client.SearchAsync<object>();
            response.TryGetOriginalException(out var ex);
        }
        catch (Exception outerEx)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchSyncWithoutErrorCheck()
    {
        var original = @"
using System;
using Elastic.Clients.Elasticsearch;

class Test
{
    void Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = {|#0:client.Search<object>()|};
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of Search was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_TryCatchWithIsValidCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticClient();
            var response = await client.SearchAsync<object>(s => s);
            if (response.IsValid) { }
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_TryCatchWithServerErrorCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticClient();
            var response = await client.SearchAsync<object>(s => s);
            var err = response.ServerError;
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_TryCatchWithOriginalExceptionCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticClient();
            var response = await client.SearchAsync<object>(s => s);
            var ex = response.OriginalException;
        }
        catch (Exception outerEx)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_Nest_TryCatchWithoutErrorCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticClient();
            var response = await {|#0:client.SearchAsync<object>(s => s)|};
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchResponseDiscarded()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            await {|#0:client.SearchAsync<object>()|};
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_NotInsideTryCatch_ResponseAssigned()
    {
        var original = @"
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        var response = await client.SearchAsync<object>();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithApiCallCheck_Nest()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Nest;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticClient();
            var response = await client.SearchAsync<object>(s => s);
            var call = response.ApiCall;
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_OpenSearch_TryCatchWithoutErrorCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using OpenSearch.Client;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new OpenSearchClient();
            var response = await {|#0:client.SearchAsync<object>(s => s)|};
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_OpenSearch_TryCatchWithIsValidCheck()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using OpenSearch.Client;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new OpenSearchClient();
            var response = await client.SearchAsync<object>(s => s);
            if (response.IsValid) { }
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task RedisResponseNotHandled_TryCatchWithResponseUsedInUnrelatedWay()
    {
        var original = @"
using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        try
        {
            var client = new ElasticsearchClient();
            var response = await {|#0:client.SearchAsync<object>()|};
            var str = response.ToString();
        }
        catch (Exception ex)
        {
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("The response of SearchAsync was not checked for errors. Redis does not throw exceptions on failed commands when using batches/transactions, use the response object to check for errors"));
    }
}