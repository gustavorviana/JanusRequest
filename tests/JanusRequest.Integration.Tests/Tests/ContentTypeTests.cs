using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class ContentTypeTests
{
    private readonly TestServerFixture _fixture;

    public ContentTypeTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task XmlContentType_PostAndDeserialize_RoundTrips()
    {
        using var client = CreateClient();
        var request = new XmlItemRequest
        {
            Name = "XmlTest",
            Value = 42
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("XmlTest", response.Data.Name);
        Assert.Equal(42, response.Data.Value);
    }

    [Fact]
    public async Task FormUrlEncoded_PostAndReadResponse()
    {
        using var client = CreateClient();
        var request = new FormUrlEncodedRequest
        {
            Username = "admin",
            Password = "secret123"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.Fields.ContainsKey("Username"));
        Assert.Equal("admin", response.Data.Fields["Username"]);
        Assert.True(response.Data.Fields.ContainsKey("Password"));
        Assert.Equal("secret123", response.Data.Fields["Password"]);
    }
}
