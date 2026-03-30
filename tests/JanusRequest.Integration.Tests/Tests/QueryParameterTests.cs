using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class QueryParameterTests
{
    private readonly TestServerFixture _fixture;

    public QueryParameterTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task QueryArgAttribute_AppendsQueryParameters()
    {
        using var client = CreateClient();
        var request = new QueryTestRequest
        {
            Page = 2,
            Size = 10,
            Search = "test"
        };

        var response = await client.GetAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("2", response.Data.Values["page"]);
        Assert.Equal("10", response.Data.Values["size"]);
        Assert.Equal("test", response.Data.Values["search"]);
    }

    [Fact]
    public async Task DefaultArgs_AppendedToAllRequests()
    {
        using var client = CreateClient();
        client.DefaultArgs.Set("apiVersion", "2");
        client.DefaultArgs.Set("format", "json");

        var response = await client.GetAsync<EchoResponse>("/api/echo/query");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("2", response.Data.Values["apiVersion"]);
        Assert.Equal("json", response.Data.Values["format"]);
    }
}
