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

    [Fact]
    public async Task SendDataAsync_Delete204NoContent_ReturnsNull()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo { Path = "/api/items/1", Method = "DELETE" };

        var result = await client.SendDataAsync<ItemResponse>(info);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteDataAsync_204NoContent_ReturnsNull()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo { Path = "/api/items/2", Method = "DELETE" };

        var result = await client.SendDataAsync<ItemResponse>(info);

        Assert.Null(result);
    }

    [Fact]
    public void AuthChaining_IHttpApiDataClient_ReturnsSameInterface()
    {
        using var client = CreateClient();
        IHttpApiDataClient dataClient = client;

        var result = dataClient.SetBearerAuthentication("test-token");

        Assert.IsAssignableFrom<IHttpApiDataClient>(result);
        Assert.Same(client, result);
    }

    [Fact]
    public void AuthChaining_IHttpApiClient_ReturnsSameInterface()
    {
        using var client = CreateClient();
        IHttpApiClient apiClient = client;

        var result = apiClient.SetBearerAuthentication("test-token");

        Assert.IsAssignableFrom<IHttpApiClient>(result);
        Assert.Same(client, result);
    }

    [Fact]
    public void AuthChaining_IHttpApiDataClient_AllMethodsReturnCorrectType()
    {
        using var client = CreateClient();
        IHttpApiDataClient dataClient = client;

        Assert.IsAssignableFrom<IHttpApiDataClient>(dataClient.SetBasicAuthentication("user", "pass"));
        Assert.IsAssignableFrom<IHttpApiDataClient>(dataClient.SetBearerAuthentication("token"));
        Assert.IsAssignableFrom<IHttpApiDataClient>(dataClient.SetApiKeyAuthentication("key"));
        Assert.IsAssignableFrom<IHttpApiDataClient>(dataClient.SetAuthentication("Custom", "value"));
        Assert.IsAssignableFrom<IHttpApiDataClient>(dataClient.ClearAuthentication());
    }

    [Fact]
    public async Task PostDataAsync_WithBearerAuth_SendsAuthenticatedRequest()
    {
        using var client = CreateClient();
        IHttpApiDataClient dataClient = client;

        dataClient.SetBearerAuthentication("my-secret-token");
        var result = await client.GetDataAsync<AuthInfoResponse>("/api/auth/bearer");

        Assert.NotNull(result);
        Assert.Equal("Bearer", result.Scheme);
        Assert.Equal("my-secret-token", result.Token);
    }
}
