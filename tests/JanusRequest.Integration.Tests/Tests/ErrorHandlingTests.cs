using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;
using JanusRequest.HttpHandlers;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class ErrorHandlingTests
{
    private readonly TestServerFixture _fixture;

    public ErrorHandlingTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClientWithErrorHandler()
    {
        var client = new HttpApiClient(_fixture.BaseUrl);
        client.Settings = new HttpApiClientSettings().SetHandlers(new HttpErrorHandler());
        return client;
    }

    [Fact]
    public async Task HttpErrorHandler_404_ThrowsRequestException()
    {
        using var client = CreateClientWithErrorHandler();

        var ex = await Assert.ThrowsAsync<RequestException>(
            () => client.GetAsync<ErrorResponse>("/api/errors/not-found"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Contains("Not found", ex.Response);
    }

    [Fact]
    public async Task HttpErrorHandler_500_ThrowsRequestException()
    {
        using var client = CreateClientWithErrorHandler();

        var ex = await Assert.ThrowsAsync<RequestException>(
            () => client.GetAsync<ErrorResponse>("/api/errors/server-error"));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Contains("Internal server error", ex.Response);
    }

    [Fact]
    public async Task HttpErrorHandler_429_ThrowsThrottlingException()
    {
        using var client = CreateClientWithErrorHandler();

        var ex = await Assert.ThrowsAsync<ThrottlingException>(
            () => client.GetAsync<ErrorResponse>("/api/errors/throttled"));

        Assert.Equal(5, ex.RetryAfter);
        Assert.Equal(100, ex.RequestLimit);
    }

    [Fact]
    public async Task HttpErrorHandler_401_ThrowsUnauthorizedAccessException()
    {
        using var client = CreateClientWithErrorHandler();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => client.GetAsync<AuthInfoResponse>("/api/auth/bearer"));
    }

    [Fact]
    public async Task NoHandler_NonSuccessResponse_ReturnsWithoutException()
    {
        using var client = new HttpApiClient(_fixture.BaseUrl);
        // No error handler registered

        var response = await client.GetAsync<ErrorResponse>("/api/errors/not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        // Without handler, no exception is thrown - response is returned as-is
    }
}
