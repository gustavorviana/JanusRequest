using JanusRequest.Attributes;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientSendTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task GetAsync_WithValidRequest_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.Name);
        }

        [Fact]
        public async Task GetAsync_WithNoContentResponse_ReturnsNullDataAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.NoContent, "");

            // Act
            var result = await _httpApiClient.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, result.Status);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task PostAsync_WithValidRequest_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.Created, "{\"Id\":2,\"Name\":\"Created\"}");

            // Act
            var result = await _httpApiClient.PostAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, result.Status);
            Assert.Equal(2, result.Data.Id);
            Assert.Equal("Created", result.Data.Name);
        }

        [Fact]
        public async Task PutAsync_WithValidRequest_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":3,\"Name\":\"Updated\"}");

            // Act
            var result = await _httpApiClient.PutAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(3, result.Data.Id);
            Assert.Equal("Updated", result.Data.Name);
        }

        [Fact]
        public async Task DeleteAsync_WithValidRequest_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":4,\"Name\":\"Deleted\"}");

            // Act
            var result = await _httpApiClient.DeleteAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(4, result.Data.Id);
        }

        [Fact]
        public async Task PatchAsync_WithValidRequest_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":5,\"Name\":\"Patched\"}");

            // Act
            var result = await _httpApiClient.PatchAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(5, result.Data.Id);
            Assert.Equal("Patched", result.Data.Name);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task SendAsync_WithDifferentMethods_UsesCorrectHttpMethodAsync(string httpMethod)
        {
            // Arrange
            var request = new TestRequest();
            var info = new HttpRequestInfo { Method = httpMethod };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request, info);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req => req.Method.Method == httpMethod),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendRequestAsync_WithNullUrlAndEmptyPath_ThrowsInvalidOperationExceptionAsync()
        {
            var clientWithoutUrl = new HttpApiClient((string)null) { Settings = _settings };
            await Assert.ThrowsAsync<InvalidOperationException>(() => clientWithoutUrl.SendRequestAsync(""));
            clientWithoutUrl.Dispose();
        }

        [Fact]
        public async Task SendHttpRequestAsync_ReturnsHttpResponseMessageAsync()
        {
            // Arrange
            var requestBody = new { Id = 1, Name = "Test" };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _httpMessageHandler.OnSendedAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _httpApiClient.SendHttpRequestAsync(requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Same(expectedResponse, result);
        }

        [Fact]
        public void Get_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = _httpApiClient.Get(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public void Post_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.Created, "{\"Id\":2,\"Name\":\"Created\"}");

            // Act
            var result = _httpApiClient.Post(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, result.Status);
            Assert.Equal(2, result.Data.Id);
        }

        [Fact]
        public void Put_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":3,\"Name\":\"Updated\"}");

            // Act
            var result = _httpApiClient.Put(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(3, result.Data.Id);
        }

        [Fact]
        public void Delete_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":4,\"Name\":\"Deleted\"}");

            // Act
            var result = _httpApiClient.Delete(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(4, result.Data.Id);
        }

        [Fact]
        public void Patch_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":5,\"Name\":\"Patched\"}");

            // Act
            var result = _httpApiClient.Patch(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(5, result.Data.Id);
        }

        [Fact]
        public void Send_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = _httpApiClient.Send(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public void SendRequest_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var requestBody = new { Id = 1, Name = "Test" };
            SetupHttpResponse(HttpStatusCode.OK, "");

            // Act
            var result = _httpApiClient.SendRequest(requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public void SendWebRequest_SyncVersion_ReturnsHttpResponseMessage()
        {
            // Arrange
            var requestBody = new { Id = 1, Name = "Test" };
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            _httpMessageHandler.OnSendedAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = _httpApiClient.SendWebRequest(requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Same(expectedResponse, result);
        }

        [Fact]
        public async Task GetAsync_BodilessStringUrl_SendsGetToCorrectUrlAsync()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.GetAsync<TestResponse>("/api/items");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.Name);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "GET" &&
                    req.RequestUri!.ToString().Contains("api/items")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAsync_BodilessStringUrl_HasNoRequestBodyAsync()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.GetAsync<TestResponse>("/api/items");

            // Assert
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req => req.Content == null),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAsync_BodilessHttpRequestInfo_ForcesGetMethodAsync()
        {
            // Arrange
            var info = new HttpRequestInfo { Method = "POST", Path = "/api/items" };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.GetAsync<TestResponse>(info);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "GET" &&
                    req.RequestUri!.ToString().Contains("api/items")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GetAsync_BodilessHttpRequestInfo_PreservesOriginalInfoMethodAsync()
        {
            // Arrange - Verify Clone doesn't mutate the original
            var info = new HttpRequestInfo { Method = "POST", Path = "/api/items" };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.GetAsync<TestResponse>(info);

            // Assert - Original info should still have POST
            Assert.Equal("POST", info.Method);
        }

        [Fact]
        public async Task GetAsync_BodilessHttpRequestInfo_HasNoRequestBodyAsync()
        {
            // Arrange
            var info = new HttpRequestInfo { Path = "/api/items" };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.GetAsync<TestResponse>(info);

            // Assert
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req => req.Content == null),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Get_BodilessSyncStringUrl_ReturnsResponse()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = _httpApiClient.Get<TestResponse>("/api/items");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.Name);
        }

        [Fact]
        public void Get_BodilessSyncHttpRequestInfo_ReturnsResponse()
        {
            // Arrange
            var info = new HttpRequestInfo { Method = "PUT", Path = "/api/items" };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":2,\"Name\":\"Synced\"}");

            // Act
            var result = _httpApiClient.Get<TestResponse>(info);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(2, result.Data.Id);
            Assert.Equal("Synced", result.Data.Name);
        }

        // US-012: Unit tests for body + string URL overloads

        [Fact]
        public async Task GetAsync_BodyStringUrl_SendsGetWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.GetAsync(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "GET" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PostAsync_BodyStringUrl_SendsPostWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":2,\"Name\":\"Posted\"}");

            // Act
            var result = await _httpApiClient.PostAsync(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(2, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "POST" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PutAsync_BodyStringUrl_SendsPutWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":3,\"Name\":\"Updated\"}");

            // Act
            var result = await _httpApiClient.PutAsync(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(3, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "PUT" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_BodyStringUrl_SendsDeleteWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":4,\"Name\":\"Deleted\"}");

            // Act
            var result = await _httpApiClient.DeleteAsync(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(4, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "DELETE" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PatchAsync_BodyStringUrl_SendsPatchWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":5,\"Name\":\"Patched\"}");

            // Act
            var result = await _httpApiClient.PatchAsync(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(5, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "PATCH" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_BodyStringUrl_SendsCorrectMethodWithBodyToCorrectUrlAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":6,\"Name\":\"Sent\"}");

            // Act
            var result = await _httpApiClient.SendAsync("PUT", request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(6, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "PUT" &&
                    req.RequestUri!.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Get_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = _httpApiClient.Get(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
        }

        [Fact]
        public void Post_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":2,\"Name\":\"Posted\"}");

            // Act
            var result = _httpApiClient.Post(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(2, result.Data.Id);
        }

        [Fact]
        public void Put_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":3,\"Name\":\"Updated\"}");

            // Act
            var result = _httpApiClient.Put(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(3, result.Data.Id);
        }

        [Fact]
        public void Delete_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":4,\"Name\":\"Deleted\"}");

            // Act
            var result = _httpApiClient.Delete(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(4, result.Data.Id);
        }

        [Fact]
        public void Patch_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":5,\"Name\":\"Patched\"}");

            // Act
            var result = _httpApiClient.Patch(request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(5, result.Data.Id);
        }

        [Fact]
        public void Send_SyncBodyStringUrl_ReturnsResponse()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":6,\"Name\":\"Sent\"}");

            // Act
            var result = _httpApiClient.Send("PUT", request, "/api/custom");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(6, result.Data.Id);
        }

        [Fact]
        public async Task SendAsync_WithHttpRequestInfo_ReturnsResponseAsync()
        {
            // Arrange
            var info = new HttpRequestInfo { Method = "GET" };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync<TestResponse>(info);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.Name);
        }

        [Fact]
        public async Task SendAsync_WithStringHttpMethod_ReturnsResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync("GET", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            Assert.Equal("Test", result.Data.Name);
        }

        [Fact]
        public async Task SendAsync_WithUrlBodyAndMethod_UsesCorrectMethodAndPathAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request, "/api/custom", "POST");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "POST" &&
                    req.RequestUri.ToString().Contains("api/custom")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_WithUrlAndBody_UsesGetByDefaultAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request, "/api/users", "GET");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, result.Data.Id);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Method.Method == "GET" &&
                    req.RequestUri.ToString().Contains("api/users")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_WithUnsupportedContentType_ThrowsNotSupportedExceptionAsync()
        {
            // Arrange
            var request = new UnsupportedContentTypeRequest();
            SetupHttpResponse(HttpStatusCode.OK, "test");

            // Act & Assert
            await Assert.ThrowsAsync<System.Text.Json.JsonException>(async () =>
                await _httpApiClient.SendAsync(request));
        }

        [Theory]
        [InlineData("GET", false)]
        [InlineData("DELETE", false)]
        [InlineData("POST", true)]
        [InlineData("PUT", true)]
        [InlineData("PATCH", true)]
        [InlineData("HEAD", false)]
        [InlineData("OPTIONS", false)]
        public void CreateHttpRequestMessage_WithBodyAndMethod_AddsBodyOnlyForSupportedMethods(string method, bool shouldHaveBody)
        {
            // Arrange
            var request = new TestRequest();
            var info = new HttpRequestInfo { Method = method };

            // Act
            var httpRequest = _httpApiClient.CreateHttpRequestMessage(info, request);

            // Assert
            if (shouldHaveBody)
                Assert.NotNull(httpRequest.Content);
            else
                Assert.Null(httpRequest.Content);

            httpRequest.Dispose();
        }

        [Theory]
        [InlineData("GET", NonStandardBodyMethods.None, false)]
        [InlineData("GET", NonStandardBodyMethods.Get, true)]
        [InlineData("DELETE", NonStandardBodyMethods.None, false)]
        [InlineData("DELETE", NonStandardBodyMethods.Delete, true)]
        public void CreateHttpRequestMessage_WithNonStandardBodyFlags_RespectsConfiguration(
            string method,
            NonStandardBodyMethods flags,
            bool shouldHaveBody)
        {
            // Arrange
            var request = new TestRequest();
            var info = new HttpRequestInfo
            {
                Method = method,
                AllowNonStandardBody = flags,
            };

            // Act
            var httpRequest = _httpApiClient.CreateHttpRequestMessage(info, request);

            // Assert
            if (shouldHaveBody)
                Assert.NotNull(httpRequest.Content);
            else
                Assert.Null(httpRequest.Content);

            httpRequest.Dispose();
        }

        [Fact]
        public async Task SendAsync_WithCancellationToken_PassesTokenToHttpClientAsync()
        {
            // Arrange
            var request = new TestRequest();
            var cancellationTokenSource = new CancellationTokenSource();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request, cancellationToken: cancellationTokenSource.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_WithCustomHttpRequestInfo_UsesProvidedInfoAsync()
        {
            // Arrange
            var request = new TestRequest();
            var customInfo = new HttpRequestInfo
            {
                Method = "PATCH",
                Headers = new System.Collections.Specialized.NameValueCollection { ["Custom-Header"] = "CustomValue" }
            };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request, customInfo);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req => req.Method.Method == "PATCH"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendRequestAsync_WithObject_ReturnsBasicResponseAsync()
        {
            // Arrange
            var requestBody = new { Id = 1, Name = "Test" };
            SetupHttpResponse(HttpStatusCode.OK, "");

            // Act
            var result = await _httpApiClient.SendRequestAsync(requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
        [Fact]
        public async Task SendAsync_WithHeaderAttributeOnRequest_HeaderAppearsOnHttpRequestMessageAsync()
        {
            // Arrange
            var request = new TestRequestWithHeaders
            {
                Token = "my-secret-token",
                Name = "Test"
            };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.PostAsync(request);

            // Assert
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("X-Auth-Token") &&
                    req.Headers.GetValues("X-Auth-Token").First() == "my-secret-token"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_WithHeaderCollectionAttributeOnRequest_HeadersAppearOnHttpRequestMessageAsync()
        {
            // Arrange
            var request = new TestRequestWithHeaderCollectionE2E
            {
                CustomHeaders = new Dictionary<string, string>
                {
                    ["X-First"] = "value1",
                    ["X-Second"] = "value2"
                }
            };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.PostAsync(request);

            // Assert
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Headers.Contains("X-First") &&
                    req.Headers.GetValues("X-First").First() == "value1" &&
                    req.Headers.Contains("X-Second") &&
                    req.Headers.GetValues("X-Second").First() == "value2"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_WithHeaderAttribute_ExcludesFromJsonBodyAsync()
        {
            // Arrange
            var request = new TestRequestWithHeaders
            {
                Token = "my-secret-token",
                Name = "TestBody"
            };
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            await _httpApiClient.PostAsync(request);

            // Assert
            await _httpMessageHandler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(req =>
                    req.Content != null &&
                    !req.Content.ReadAsStringAsync().Result.Contains("my-secret-token") &&
                    req.Content.ReadAsStringAsync().Result.Contains("TestBody")),
                Arg.Any<CancellationToken>());
        }

        [Request("http://localhost/test", Method = "POST")]
        private class TestRequestWithHeaders : IRequestResponse<TestResponse>
        {
            [Header("X-Auth-Token")]
            public string Token { get; set; }

            public string Name { get; set; }
        }

        [Request("http://localhost/test", Method = "POST")]
        private class TestRequestWithHeaderCollectionE2E : IRequestResponse<TestResponse>
        {
            [HeaderCollection]
            public Dictionary<string, string> CustomHeaders { get; set; }
        }
    }
}
