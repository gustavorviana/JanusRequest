using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class FileUploadTests
{
    private readonly TestServerFixture _fixture;

    public FileUploadTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task Upload_ByteArray_ReturnsFileSizeAndFields()
    {
        using var client = CreateClient();
        var fileContent = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }; // "Hello"
        var request = new FileUploadByteArrayRequest
        {
            File = fileContent,
            Description = "test-upload"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(5, response.Data.Size);
        Assert.True(response.Data.Fields.ContainsKey("description"));
        Assert.Equal("test-upload", response.Data.Fields["description"]);
    }

    [Fact]
    public async Task Upload_Stream_ReturnsFileSizeAndFields()
    {
        using var client = CreateClient();
        var fileContent = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var request = new FileUploadStreamRequest
        {
            File = new MemoryStream(fileContent),
            Description = "stream-upload"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(10, response.Data.Size);
        Assert.True(response.Data.Fields.ContainsKey("description"));
        Assert.Equal("stream-upload", response.Data.Fields["description"]);
    }
}
