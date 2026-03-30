using System.Net;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class CrudTests
{
    private readonly TestServerFixture _fixture;

    public CrudTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    [Fact]
    public async Task GetAsync_ItemList_ReturnsAllItems()
    {
        using var client = CreateClient();

        var response = await client.GetAsync<ItemResponse[]>("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(3, response.Data.Length);
        Assert.Equal("Item1", response.Data[0].Name);
    }

    [Fact]
    public async Task GetAsync_SingleItem_ReturnsItem()
    {
        using var client = CreateClient();

        var response = await client.GetAsync<ItemResponse>("/api/items/1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
        Assert.Equal("Item1", response.Data.Name);
    }

    [Fact]
    public async Task PostAsync_CreateItem_Returns201WithCreatedItem()
    {
        using var client = CreateClient();
        var request = new CreateItemRequest { Name = "NewItem" };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("NewItem", response.Data.Name);
        Assert.True(response.Data.Id > 0);
    }

    [Fact]
    public async Task PutAsync_UpdateItem_ReturnsUpdatedItem()
    {
        using var client = CreateClient();
        var request = new UpdateItemRequest { Id = 1, Name = "UpdatedItem" };

        var response = await client.PutAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
        Assert.Equal("UpdatedItem", response.Data.Name);
    }

    [Fact]
    public async Task PatchAsync_PatchItem_ReturnsPatchedItem()
    {
        using var client = CreateClient();
        var request = new PatchItemRequest { Id = 2, Name = "PatchedItem" };

        var response = await client.PatchAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Id);
        Assert.Equal("PatchedItem", response.Data.Name);
    }

    [Fact]
    public async Task DeleteAsync_Item_Returns204NoContent()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo { Path = "/api/items/1", Method = "DELETE" };

        var response = await client.SendHttpRequestAsync(null, info);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_GenericPost_CreatesItem()
    {
        using var client = CreateClient();
        var request = new CreateItemRequest { Name = "GenericSend" };

        var response = await client.SendAsync(request, "/api/items", "POST");

        Assert.Equal(HttpStatusCode.Created, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("GenericSend", response.Data.Name);
    }
}
