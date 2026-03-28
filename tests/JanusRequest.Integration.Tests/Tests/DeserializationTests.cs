using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class DeserializationTests
{
    private readonly TestServerFixture _fixture;

    public DeserializationTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task MalformedJson_ThrowsDeserializationException()
    {
        using var client = CreateClient();

        var ex = await Assert.ThrowsAsync<DeserializationException>(
            () => client.GetAsync<ItemResponse>("/api/errors/bad-json"));

        Assert.Equal(typeof(ItemResponse), ex.TargetType);
        Assert.Contains("not valid json", ex.Content);
    }
}
