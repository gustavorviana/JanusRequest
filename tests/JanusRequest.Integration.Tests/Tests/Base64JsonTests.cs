using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class Base64JsonTests
{
    private readonly TestServerFixture _fixture;

    public Base64JsonTests(TestServerFixture fixture) => _fixture = fixture;

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task PostAsync_ByteArray_SendsAsBase64AndDeserializesResponse()
    {
        using var client = CreateClient();
        var imageBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }; // PNG header
        var request = new Base64JsonByteArrayRequest
        {
            Name = "test-image",
            Image = imageBytes
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("test-image", response.Data.Name);
        Assert.Equal(imageBytes, response.Data.Image);
    }

    [Fact]
    public async Task PostAsync_Stream_SendsAsBase64AndDeserializesResponse()
    {
        using var client = CreateClient();
        var imageBytes = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
        var request = new Base64JsonStreamRequest
        {
            Name = "test-stream",
            Image = new MemoryStream(imageBytes)
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("test-stream", response.Data.Name);
        Assert.Equal(imageBytes, response.Data.Image);
    }

    [Fact]
    public async Task PostAsync_NullByteArray_SendsNullAndDeserializesResponse()
    {
        using var client = CreateClient();
        var request = new Base64JsonByteArrayRequest
        {
            Name = "no-image",
            Image = null
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("no-image", response.Data.Name);
        Assert.Null(response.Data.Image);
    }
}
