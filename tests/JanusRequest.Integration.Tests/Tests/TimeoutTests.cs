using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class TimeoutTests
{
    private readonly TestServerFixture _fixture;

    public TimeoutTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CancellationToken_CancelsLongRunningRequest()
    {
        using var client = new HttpApiClient(_fixture.BaseUrl);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync<ItemResponse>("/api/slow", cts.Token));
    }

    [Fact]
    public async Task HttpClientTimeout_ThrowsOnSlowResponse()
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(200) };
        httpClient.BaseAddress = new Uri(_fixture.BaseUrl);
        using var client = new HttpApiClient(httpClient, disposeHttpClient: false);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync<ItemResponse>("/api/slow"));
    }
}
