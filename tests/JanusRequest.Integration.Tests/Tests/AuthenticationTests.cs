using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class AuthenticationTests
{
    private readonly TestServerFixture _fixture;

    public AuthenticationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task SetBearerAuthentication_SendsBearerToken()
    {
        using var client = CreateClient();
        client.SetBearerAuthentication("my-secret-token");

        var response = await client.GetAsync<AuthInfoResponse>("/api/auth/bearer");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Bearer", response.Data.Scheme);
        Assert.Equal("my-secret-token", response.Data.Token);
    }

    [Fact]
    public async Task SetBasicAuthentication_SendsEncodedCredentials()
    {
        using var client = CreateClient();
        client.SetBasicAuthentication("admin", "password123");

        var response = await client.GetAsync<AuthInfoResponse>("/api/auth/basic");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Basic", response.Data.Scheme);
        Assert.Equal("admin", response.Data.User);
        Assert.Equal("password123", response.Data.Pass);
    }

    [Fact]
    public async Task SetApiKeyAuthentication_SendsApiKeyHeader()
    {
        using var client = CreateClient();
        client.SetApiKeyAuthentication("key-abc-123");

        var response = await client.GetAsync<AuthInfoResponse>("/api/auth/apikey");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("key-abc-123", response.Data.ApiKey);
    }

    [Fact]
    public async Task SetApiKeyAuthentication_CustomHeaderName_SendsCorrectHeader()
    {
        using var client = CreateClient();
        client.SetApiKeyAuthentication("key-custom-456", "Authorization-Key");

        var response = await client.GetAsync<AuthInfoResponse>("/api/auth/apikey");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("key-custom-456", response.Data.ApiKey);
    }

    [Fact]
    public async Task ClearAuthentication_RemovesAuthHeader_Returns401()
    {
        using var client = CreateClient();
        client.SetBearerAuthentication("some-token");
        client.ClearAuthentication();

        var response = await client.SendHttpRequestAsync(null, new HttpRequestInfo { Path = "/api/auth/bearer", Method = "GET" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
