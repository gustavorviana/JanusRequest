using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;
using JanusRequest.HttpHandlers;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class RetryTests
{
    private readonly TestServerFixture _fixture;

    public RetryTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient()
    {
        var client = new HttpApiClient(_fixture.BaseUrl);
        client.Settings = new HttpApiClientSettings().SetHandlers(
            new ThrottleRetryHandler(
                maxRetries: 5,
                baseDelaySeconds: 0.01,
                maxDelaySeconds: 0.1));
        return client;
    }

    [Fact]
    public async Task ThrottleRetryHandler_RetriesOn429_EventuallySucceeds()
    {
        using var client = CreateClient();
        var key = Guid.NewGuid().ToString();

        var response = await client.GetAsync<ItemResponse>($"/api/retry/throttle?key={key}&failUntil=2");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Success", response.Data.Name);
    }

    [Fact]
    public async Task ThrottleRetryHandler_RetriesOn503_EventuallySucceeds()
    {
        using var client = CreateClient();
        var key = Guid.NewGuid().ToString();

        var response = await client.GetAsync<ItemResponse>($"/api/retry/transient?key={key}&failUntil=2");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Success", response.Data.Name);
    }

    [Fact]
    public async Task ThrottleRetryHandler_ExhaustsRetries_ReturnsLastFailedResponse()
    {
        var client = new HttpApiClient(_fixture.BaseUrl);
        client.Settings = new HttpApiClientSettings().SetHandlers(
            new ThrottleRetryHandler(
                maxRetries: 2,
                baseDelaySeconds: 0.01,
                maxDelaySeconds: 0.1));

        var key = Guid.NewGuid().ToString();

        // failUntil=10 means it will always fail within 2 retries
        var response = await client.GetAsync<ItemResponse>($"/api/retry/throttle?key={key}&failUntil=10");

        Assert.Equal((HttpStatusCode)429, response.Status);
    }

    [Fact]
    public async Task ThrottleRetryHandler_RespectsRetryAfterHeader()
    {
        using var client = CreateClient();
        var key = Guid.NewGuid().ToString();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync<ItemResponse>($"/api/retry/throttle?key={key}&failUntil=1");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, response.Status);
        // Retry-After: 0 means minimal delay, should complete fast
        Assert.True(sw.ElapsedMilliseconds < 5000, $"Expected fast retry but took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    public async Task ThrottleRetryHandler_WithJitterStrategy_EventuallySucceeds()
    {
        var client = new HttpApiClient(_fixture.BaseUrl);
        client.Settings = new HttpApiClientSettings().SetHandlers(
            new ThrottleRetryHandler(
                maxRetries: 5,
                baseDelaySeconds: 0.01,
                maxDelaySeconds: 0.1,
                RetryDelayStrategy.Jitter));

        var key = Guid.NewGuid().ToString();

        var response = await client.GetAsync<ItemResponse>($"/api/retry/throttle?key={key}&failUntil=2");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("Success", response.Data.Name);
    }
}
