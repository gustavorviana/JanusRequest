using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class DataAsyncTests
{
    private readonly TestServerFixture _fixture;

    public DataAsyncTests(TestServerFixture fixture) => _fixture = fixture;

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task GetDataAsync_ReturnsDeserializedData()
    {
        using var client = CreateClient();

        var items = await client.GetDataAsync<ItemResponse[]>("/api/items");

        Assert.NotNull(items);
        Assert.True(items.Length > 0);
    }

    [Fact]
    public async Task GetDataAsync_NonExistentResource_ThrowsRequestException()
    {
        using var client = CreateClient();

        var ex = await Assert.ThrowsAsync<RequestException>(
            () => client.GetDataAsync<ItemResponse>("/api/items/999"));

        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task PostDataAsync_ReturnsCreatedItem()
    {
        using var client = CreateClient();
        var request = new CreateItemRequest { Name = "DataAsyncTest" };

        var item = await client.PostDataAsync(request);

        Assert.NotNull(item);
        Assert.Equal("DataAsyncTest", item.Name);
    }

    [Fact]
    public async Task PutDataAsync_ReturnsUpdatedItem()
    {
        using var client = CreateClient();
        var request = new UpdateItemRequest { Id = 1, Name = "Updated" };

        var item = await client.PutDataAsync(request);

        Assert.NotNull(item);
        Assert.Equal(1, item.Id);
        Assert.Equal("Updated", item.Name);
    }

    [Fact]
    public async Task PatchDataAsync_ReturnsUpdatedItem()
    {
        using var client = CreateClient();
        var request = new PatchItemRequest { Id = 2, Name = "Patched" };

        var item = await client.PatchDataAsync(request);

        Assert.NotNull(item);
        Assert.Equal(2, item.Id);
        Assert.Equal("Patched", item.Name);
    }

    [Fact]
    public async Task SendDataAsync_WithBody_ReturnsData()
    {
        using var client = CreateClient();
        var request = new CreateItemRequest { Name = "SendDataTest" };

        var item = await client.SendDataAsync(request);

        Assert.NotNull(item);
        Assert.Equal("SendDataTest", item.Name);
    }

    [Fact]
    public async Task GetDataAsync_ServerError_ThrowsRequestException()
    {
        using var client = CreateClient();

        var ex = await Assert.ThrowsAsync<RequestException>(
            () => client.GetDataAsync<ErrorResponse>("/api/errors/server-error"));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }
}
