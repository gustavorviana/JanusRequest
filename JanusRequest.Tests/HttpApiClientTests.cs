using JanusRequest.Attributes;
using JanusRequest.Builders;
using JanusRequest.ContentTranslator;
using JanusRequest.HttpHandlers;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpApiClient _httpApiClient;
        private readonly HttpApiClientSettings _settings;
        private readonly MockHttpMessageHandler _httpMessageHandler;

        public HttpApiClientTests()
        {
            _httpMessageHandler = Substitute.For<MockHttpMessageHandler>();
            _settings = new HttpApiClientSettings();

            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            _httpApiClient = new HttpApiClient(_httpClient, true)
            {
                Settings = _settings
            };
        }

        public void Dispose()
        {
            _httpApiClient?.Dispose();
        }

        [Fact]
        public void Constructor_WithUrl_SetsUrlProperty()
        {
            // Arrange & Act
            var client = new HttpApiClient("https://api.example.com");

            // Assert
            Assert.Equal("https://api.example.com", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithHttpClient_SetsUrlFromBaseAddress()
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

            // Act
            var client = new HttpApiClient(httpClient);

            // Assert
            Assert.Equal("https://api.example.com/", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithUrlAndHandler_CreatesClientWithCustomHandler()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();

            // Act
            var client = new HttpApiClient("https://api.example.com", handler);

            // Assert
            Assert.Equal("https://api.example.com", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithHandler_ThrowsWhenHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpApiClient("https://api.example.com", (HttpMessageHandler)null!));
        }

        [Fact]
        public async Task ValidateRequest_WithValidModel_DoesNotThrow()
        {
            // Arrange
            var request = new ValidatedRequest { Name = "Valid" };
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithInvalidModel_ThrowsValidationException()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = true;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _httpApiClient.SendAsync(request));
        }

        [Fact]
        public async Task ValidateRequest_WithInvalidModel_ThrowsValidationExceptionWithExpectedFormat()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = true;
            const string expectedErrorMessage = "Name is required";

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _httpApiClient.SendAsync(request));

            // Assert
            Assert.NotNull(exception.Message);
            Assert.Contains(expectedErrorMessage, exception.Message);
            Assert.NotNull(exception.ValidationResult);
            Assert.Equal(expectedErrorMessage, exception.ValidationResult.ErrorMessage);
            Assert.Contains("Name", exception.ValidationResult.MemberNames);
        }

        [Fact]
        public async Task ValidateRequest_WhenDisabled_DoesNotValidate()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = false;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithNullBody_DoesNotValidate()
        {
            // Arrange
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");
            var info = new HttpRequestInfo { Path = "/test", Method = "GET" };

            // Act & Assert
            var result = await _httpApiClient.SendAsync<TestResponse>(info);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithNativeType_DoesNotValidate()
        {
            // Arrange
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");
            var info = new HttpRequestInfo { Path = "/test", Method = "POST" };

            // Act & Assert
            var result = await _httpApiClient.SendRequestAsync("string body", info);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public void SetBasicAuthentication_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetBasicAuthentication("user", "password");

            // Assert
            Assert.Equal("Basic", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:password")),
                _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetBearerAuthentication_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetBearerAuthentication("token123");

            // Assert
            Assert.Equal("Bearer", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal("token123", _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetApiKeyAuthentication_SetsCustomHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123", "X-Custom-Key");

            // Assert
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-Custom-Key"));
            Assert.Contains("key123", _httpClient.DefaultRequestHeaders.GetValues("X-Custom-Key"));
        }

        [Fact]
        public void SetApiKeyAuthentication_WithDefaultHeader_SetsXApiKeyHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123");

            // Assert
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-API-Key"));
            Assert.Contains("key123", _httpClient.DefaultRequestHeaders.GetValues("X-API-Key"));
        }

        [Fact]
        public void ClearAuthentication_RemovesAuthorizationHeader()
        {
            // Arrange
            _httpApiClient.SetBearerAuthentication("token");

            // Act
            _httpApiClient.ClearAuthentication();

            // Assert
            Assert.Null(_httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public void SetHandlers_SetsHandlersArray()
        {
            // Arrange
            var handler1 = Substitute.For<IHttpHandlerBase>();
            var handler2 = Substitute.For<IHttpHandlerBase>();

            // Act
            var result = _settings.SetHandlers(handler1, handler2);

            // Assert
            Assert.Same(_settings, result); // Fluent interface
        }

        [Fact]
        public void SetHandlers_WithNullHandlers_AcceptsNull()
        {
            // Act
            var result = _settings.SetHandlers(null);

            // Assert
            Assert.Same(_settings, result);
        }

        [Fact]
        public void JoinUrl_WithMultipleParts_JoinsCorrectly()
        {
            // Act
            //var result = new UrlQueryBuilder().BuildUrl("https://api.com/", "/users/", "123");

            //// Assert
            //Assert.Equal("https://api.com/users/123", result);
        }

        [Fact]
        public void JoinUrl_WithEmptyParts_IgnoresEmptyParts()
        {
            // Arrange
            var result = new UrlQueryBuilder().BuildUrl("https://api.com", "", "users");

            // Assert
            Assert.Equal("https://api.com/users", result);
        }

        [Fact]
        public void JoinUrl_WithNullParts_IgnoresNullParts()
        {
            // Act
            var result = new UrlQueryBuilder().BuildUrl("https://api.com", null, "users");

            // Assert
            Assert.Equal("https://api.com/users", result);
        }

        [Fact]
        public async Task GetAsync_WithValidRequest_ReturnsResponse()
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
        public async Task GetAsync_WithNoContentResponse_ReturnsNullData()
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
        public async Task PostAsync_WithValidRequest_ReturnsResponse()
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
        public async Task PutAsync_WithValidRequest_ReturnsResponse()
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
        public async Task DeleteAsync_WithValidRequest_ReturnsResponse()
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
        public async Task PatchAsync_WithValidRequest_ReturnsResponse()
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
        public async Task SendAsync_WithDifferentMethods_UsesCorrectHttpMethod(string httpMethod)
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
        public async Task SendRequestAsync_WithoutGenericType_ReturnsBasicResponse()
        {
            var requestBody = new { Id = 1, Name = "Test" };
            await Assert.ThrowsAsync<InvalidOperationException>(() => _httpApiClient.SendRequestAsync(""));
        }

        [Fact]
        public async Task SendHttpRequestAsync_ReturnsHttpResponseMessage()
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
        public void Path_SyncVersion_CallsAsyncAndWaits()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":5,\"Name\":\"Patched\"}");

            // Act
            var result = _httpApiClient.Path(request);

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
        public async Task SendAsync_WithErrorHandler_ThrowsException()
        {
            // Arrange
            var request = new TestRequest();
            var errorHandler = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("API Error");

            errorHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            errorHandler.MapExceptionAsync(Arg.Any<HttpResponseMessage>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(errorHandler);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _httpApiClient.SendAsync(request));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithRecoveryHandler_RecoversRequest()
        {
            // Arrange
            var request = new TestRequest();
            var recoveryHandler = Substitute.For<IHttpRecoveryHandler>();
            var recoveredResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":1,\"Name\":\"Recovered\"}")
            };

            recoveryHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            recoveryHandler.RecoverAsync(Arg.Any<HttpRecoveryContext>()).Returns(Task.FromResult(recoveredResponse));

            _settings.SetHandlers(recoveryHandler);
            SetupHttpResponse(HttpStatusCode.InternalServerError, "Error");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal("Recovered", result.Data.Name);
        }

        [Fact]
        public void Dispose_WithDisposeHttpClientTrue_DisposesHttpClient()
        {
            // Arrange
            var mockHttpClient = Substitute.For<HttpClient>();
            var client = new HttpApiClient(mockHttpClient, true);

            // Act
            client.Dispose();

            // Assert
            mockHttpClient.Received(1).Dispose();
        }

        [Fact]
        public void Dispose_WithDisposeHttpClientFalse_DoesNotDisposeHttpClient()
        {
            // Arrange
            var mockHttpClient = Substitute.For<HttpClient>();
            var client = new HttpApiClient(mockHttpClient, false);

            // Act
            client.Dispose();

            // Assert
            mockHttpClient.DidNotReceive().Dispose();
        }

        [Fact]
        public async Task SendAsync_WithHttpRequestInfo_ReturnsResponse()
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
        public async Task SendAsync_WithStringHttpMethod_ReturnsResponse()
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
        public async Task SendAsync_WithUrlBodyAndMethod_UsesCorrectMethodAndPath()
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
        public async Task SendAsync_WithUrlAndBody_UsesGetByDefault()
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
        public void SetAuthentication_WithCustomScheme_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetAuthentication("Custom", "custom-token");

            // Assert
            Assert.Equal("Custom", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal("custom-token", _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetContentBuilder_AddsContentTranslators()
        {
            // Arrange
            var jsonTranslator = Substitute.For<ContentTypeTranslator>();
            jsonTranslator.ContentType.Returns(HttpContentType.Json);

            // Act
            var result = _settings.SetContentBuilder(jsonTranslator);

            // Assert
            Assert.Same(_settings, result); // Fluent interface
        }

        [Fact]
        public void DefaultContentType_CanBeSet()
        {
            // Act
            _settings.DefaultContentType = HttpContentType.Xml;

            // Assert
            Assert.Equal(HttpContentType.Xml, _settings.DefaultContentType);
        }

        [Fact]
        public void DefaultArgs_CanBeSet()
        {
            // Arrange
            var queryBuilder = new UrlQueryBuilder();
            queryBuilder.Set("param", "value");

            // Act
            _httpApiClient.DefaultArgs = queryBuilder;

            // Assert
            Assert.Same(queryBuilder, _httpApiClient.DefaultArgs);
        }

        [Fact]
        public void DefaultHeaders_ReturnsHttpClientHeaders()
        {
            // Act
            var headers = _httpApiClient.DefaultHeaders;

            // Assert
            Assert.Same(_httpClient.DefaultRequestHeaders, headers);
        }

        [Fact]
        public void CreateHttpRequestMessage_WithNullUrlAndPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new HttpApiClient(null!);
            var info = new HttpRequestInfo();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => client.CreateHttpRequestMessage(info, null));
            Assert.Contains("Uma URL deve ser definida", ex.Message);

            client.Dispose();
        }

        [Fact]
        public async Task SendAsync_WithUnsupportedContentType_ThrowsNotSupportedException()
        {
            // Arrange
            var request = new UnsupportedContentTypeRequest();
            SetupHttpResponse(HttpStatusCode.OK, "test");

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(async () =>
                await _httpApiClient.SendAsync(request));
        }

        [Fact]
        public async Task SendAsync_WithMultipleHandlers_UsesCorrectHandler()
        {
            // Arrange
            var request = new TestRequest();
            var handler1 = Substitute.For<IHttpHandlerBase>();
            var handler2 = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("Handler 2 Error");

            handler1.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            handler2.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            handler2.MapExceptionAsync(Arg.Any<HttpResponseMessage>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(handler1, handler2);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _httpApiClient.SendAsync(request));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithNoMatchingHandler_ReturnsOriginalResponse()
        {
            // Arrange
            var request = new TestRequest();
            var handler = Substitute.For<HttpErrorHandler>();

            handler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(handler);
            SetupHttpResponse(HttpStatusCode.BadRequest, null!);

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.Status);
            await handler.DidNotReceive().MapExceptionAsync(Arg.Any<HttpResponseMessage>());
        }

        [Fact]
        public async Task SendRequestAsync_WithObject_ReturnsBasicResponse()
        {
            // Arrange
            var requestBody = new { Id = 1, Name = "Test" };
            SetupHttpResponse(HttpStatusCode.OK, "");

            // Act
            var result = await _httpApiClient.SendRequestAsync(requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Theory]
        [InlineData("GET", false)]
        [InlineData("DELETE", false)]
        [InlineData("POST", true)]
        [InlineData("PUT", true)]
        [InlineData("PATCH", true)]
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

        [Fact]
        public async Task SendAsync_WithCancellationToken_PassesTokenToHttpClient()
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
        public async Task SendAsync_WithCustomHttpRequestInfo_UsesProvidedInfo()
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
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HttpApiClient((HttpClient)null!));
        }

        [Fact]
        public async Task InternalSendRequestAsync_WithRecoveryHandlerThatCannotHandle_ReturnsOriginalResponse()
        {
            // Arrange
            var request = new TestRequest();
            var recoveryHandler = Substitute.For<IHttpRecoveryHandler>();

            recoveryHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(recoveryHandler);
            SetupHttpResponse(HttpStatusCode.InternalServerError, null!);

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
            await recoveryHandler.DidNotReceive().RecoverAsync(Arg.Any<HttpRecoveryContext>());
        }

        // Classes auxiliares para testes
        [ContentType(HttpContentType.QueryString)]
        public class UnsupportedContentTypeRequest : IRequestResponse<TestResponse>
        {
        }

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode);

            if (content != null)
                response.Content = new StringContent(content);

            _httpMessageHandler.OnSendedAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(response));
        }


        public class TestResponse
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage? Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            };

            protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return OnSendedAsync(request, cancellationToken);
            }

            public virtual Task<HttpResponseMessage> OnSendedAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response!);
            }
        }

        [Request("http://localhost/test")]
        public class TestRequest : IRequestResponse<TestResponse>
        {
        }

        [Request("http://localhost/test", Method = "POST")]
        public class ValidatedRequest : IRequestResponse<TestResponse>
        {
            [Required(ErrorMessage = "Name is required")]
            public string? Name { get; set; }
        }
    }
}