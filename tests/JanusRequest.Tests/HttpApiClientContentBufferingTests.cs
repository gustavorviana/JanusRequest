using JanusRequest.Attributes;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientContentBufferingTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendAsync_WithCustomDeserializerThatThrows_DeserializationExceptionContainsContent()
        {
            // Arrange
            var request = new FailingDeserializerRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var ex = await Assert.ThrowsAsync<DeserializationException>(
                async () => await _httpApiClient.SendAsync(request));

            // Assert - content should NOT be empty thanks to LoadIntoBufferAsync
            Assert.NotNull(ex.Content);
            Assert.NotEmpty(ex.Content);
            Assert.Contains("Id", ex.Content);
            Assert.Equal(typeof(TestResponse), ex.TargetType);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public async Task SendAsync_WithBadJson_DeserializationExceptionContainsRawContent()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "this is not json");

            // Act
            var ex = await Assert.ThrowsAsync<DeserializationException>(
                async () => await _httpApiClient.SendAsync(request));

            // Assert
            Assert.Equal("this is not json", ex.Content);
            Assert.Equal(typeof(TestResponse), ex.TargetType);
        }

        [Fact]
        public async Task SendAsync_WithHttpRequestInfo_DeserializationExceptionContainsContent()
        {
            // Arrange
            var info = new HttpRequestInfo { Path = "/test", Method = "GET" };
            SetupHttpResponse(HttpStatusCode.OK, "invalid-json-content");

            // Act
            var ex = await Assert.ThrowsAsync<DeserializationException>(
                async () => await _httpApiClient.SendAsync<TestResponse>(info));

            // Assert
            Assert.Equal("invalid-json-content", ex.Content);
            Assert.Equal(typeof(TestResponse), ex.TargetType);
        }

        [Request("http://localhost/test")]
        [ResponseDeserializer(typeof(FailingDeserializer))]
        public class FailingDeserializerRequest : IRequestResponse<TestResponse>
        {
        }

        public class FailingDeserializer : IResponseDeserializer<TestResponse>
        {
            public Task<TestResponse> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings)
            {
                throw new InvalidOperationException("Deserialization intentionally failed");
            }
        }
    }
}
