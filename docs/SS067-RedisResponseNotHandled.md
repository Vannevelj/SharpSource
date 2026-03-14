# SS067 - RedisResponseNotHandled

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

The .NET Elasticsearch and OpenSearch clients do not throw exceptions when an operation fails. Instead, they return a response object that contains error information. If the response is discarded without being checked, errors will silently go unnoticed. Always capture the response and inspect it for errors.

This analyzer supports:
- `Elastic.Clients.Elasticsearch` (v8+) — response types derived from `Elastic.Transport.TransportResponse`
- `NEST` (v7) — response types implementing `Elasticsearch.Net.IElasticsearchResponse`
- `OpenSearch.Client` — response types implementing `OpenSearch.Net.IOpenSearchResponse`

---

## Violation
```cs
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        await client.IndexAsync(new MyDocument());
    }
}
```

## Fix
```cs
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;

class Test
{
    async Task Method()
    {
        var client = new ElasticsearchClient();
        var response = await client.IndexAsync(new MyDocument());
        if (!response.IsValidResponse)
        {
            // Handle error
        }
    }
}
```
