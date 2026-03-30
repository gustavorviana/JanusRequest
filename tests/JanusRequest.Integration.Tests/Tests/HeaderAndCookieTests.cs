using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class HeaderAndCookieTests
{
    private readonly TestServerFixture _fixture;

    public HeaderAndCookieTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task HeaderAttribute_SendsCustomHeaders()
    {
        using var client = CreateClient();
        var request = new HeaderTestRequest
        {
            CustomHeader = "hello-world",
            TraceId = "trace-abc-123"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Values.ContainsKey("X-Custom-Header"));
        Assert.Equal("hello-world", response.Data.Values["X-Custom-Header"]);
        Assert.True(response.Data.Values.ContainsKey("X-Trace-Id"));
        Assert.Equal("trace-abc-123", response.Data.Values["X-Trace-Id"]);
    }

    [Fact]
    public async Task HeaderCollectionAttribute_SendsMultipleHeaders()
    {
        using var client = CreateClient();
        var request = new HeaderCollectionTestRequest
        {
            Headers = new Dictionary<string, string>
            {
                ["X-First"] = "value1",
                ["X-Second"] = "value2"
            }
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Values.ContainsKey("X-First"));
        Assert.Equal("value1", response.Data.Values["X-First"]);
        Assert.True(response.Data.Values.ContainsKey("X-Second"));
        Assert.Equal("value2", response.Data.Values["X-Second"]);
    }

    [Fact]
    public async Task CookieAttribute_SendsCookies()
    {
        using var client = CreateClient();
        var request = new CookieTestRequest
        {
            SessionId = "sess-xyz-789",
            Theme = "dark"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Values.ContainsKey("session"));
        Assert.Equal("sess-xyz-789", response.Data.Values["session"]);
        Assert.True(response.Data.Values.ContainsKey("theme"));
        Assert.Equal("dark", response.Data.Values["theme"]);
    }

    [Fact]
    public async Task CookieCollectionAttribute_SendsMultipleCookies()
    {
        using var client = CreateClient();
        var request = new CookieCollectionTestRequest
        {
            Cookies = new Dictionary<string, string>
            {
                ["auth"] = "token-abc",
                ["lang"] = "pt-BR"
            }
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Values.ContainsKey("auth"));
        Assert.Equal("token-abc", response.Data.Values["auth"]);
        Assert.True(response.Data.Values.ContainsKey("lang"));
        Assert.Equal("pt-BR", response.Data.Values["lang"]);
    }
}
