using System.Net;
using JanusRequest.Integration.Tests.Fixtures;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class FileDownloadTests
{
    private readonly TestServerFixture _fixture;

    public FileDownloadTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task Download_RawStream_ReturnsBinaryContent()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo { Path = "/api/files/download", Method = "GET" };

        using var response = await client.SendHttpRequestAsync(null, info);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(256, bytes.Length);

        // Verify known byte pattern
        for (var i = 0; i < bytes.Length; i++)
            Assert.Equal((byte)(i % 256), bytes[i]);
    }
}
