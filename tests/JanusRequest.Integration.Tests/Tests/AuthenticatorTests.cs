using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class AuthenticatorTests
{
    private readonly TestServerFixture _fixture;

    public AuthenticatorTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task Authenticator_AppliesAuthBeforeRequest()
    {
        using var client = CreateClient();
        client.Settings = new HttpApiClientSettings
        {
            Authenticator = new HttpAuthenticator("Bearer", "my-initial-token")
        };

        var response = await client.GetAsync<AuthInfoResponse>("/api/authenticator/protected");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Bearer", response.Data.Scheme);
        Assert.Equal("my-initial-token", response.Data.Token);
    }

    [Fact]
    public async Task Authenticator_HandlesUnauthorized_RetriesOnce()
    {
        var key = Guid.NewGuid().ToString();
        _fixture.ResetCallCount($"token-refresh-{key}");

        var authenticator = new JwtRefreshAuthenticator("stale-token", "refreshed-token");

        using var client = CreateClient();
        client.Settings = new HttpApiClientSettings { Authenticator = authenticator };

        var response = await client.SendAsync<AuthInfoResponse>(
            new HttpRequestInfo
            {
                Path = $"/api/authenticator/token-refresh?key={key}",
                Method = "POST"
            });

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("refreshed-token", response.Data.Token);
        Assert.Equal(1, authenticator.AuthCallCount);
        Assert.Equal(1, authenticator.RefreshCallCount);
    }

    [Fact]
    public async Task PerRequestAuthenticator_OverridesGlobal()
    {
        using var client = CreateClient();
        client.Settings = new HttpApiClientSettings
        {
            Authenticator = new HttpAuthenticator("Bearer", "global-token")
        };

        var info = new HttpRequestInfo
        {
            Path = "/api/authenticator/protected",
            Method = "GET",
            Authenticator = new HttpAuthenticator("Bearer", "per-request-token")
        };

        var response = await client.SendAsync<AuthInfoResponse>(info);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("per-request-token", response.Data.Token);
    }

    [Fact]
    public async Task Authenticator_HandlesForbidden_RetriesOnce()
    {
        var key = Guid.NewGuid().ToString();
        _fixture.ResetCallCount($"forbidden-refresh-{key}");

        var authenticator = new JwtRefreshAuthenticator("stale-token", "refreshed-token");

        using var client = CreateClient();
        client.Settings = new HttpApiClientSettings { Authenticator = authenticator };

        var response = await client.SendAsync<AuthInfoResponse>(
            new HttpRequestInfo
            {
                Path = $"/api/authenticator/forbidden-refresh?key={key}",
                Method = "POST"
            });

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("refreshed-token", response.Data.Token);
        Assert.Equal(1, authenticator.AuthCallCount);
        Assert.Equal(1, authenticator.RefreshCallCount);
    }

    /// <summary>
    /// Test-only authenticator that simulates JWT with refresh token.
    /// On first auth: uses initialToken. On 401: switches to refreshedToken.
    /// This class has business logic and belongs in tests, not in the library.
    /// </summary>
    private sealed class JwtRefreshAuthenticator : IHttpAuthenticator
    {
        private readonly string _initialToken;
        private readonly string _refreshedToken;
        private string _currentToken;

        public int AuthCallCount { get; private set; }
        public int RefreshCallCount { get; private set; }

        public JwtRefreshAuthenticator(string initialToken, string refreshedToken)
        {
            _initialToken = initialToken;
            _refreshedToken = refreshedToken;
            _currentToken = initialToken;
        }

        public Task AuthenticateAsync(HttpRequestMessage request, HttpClient httpClient)
        {
            AuthCallCount++;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
            return Task.CompletedTask;
        }

        public Task<bool> HandleUnauthorizedAsync(HttpRequestMessage request, HttpResponseMessage response, HttpClient httpClient)
        {
            RefreshCallCount++;
            _currentToken = _refreshedToken;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _currentToken);
            return Task.FromResult(true);
        }
    }
}
