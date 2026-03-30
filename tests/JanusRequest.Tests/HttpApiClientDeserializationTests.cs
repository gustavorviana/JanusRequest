using JanusRequest.Attributes;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientDeserializationTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendAsync_WithArrayResponse_DeserializesCorrectlyAsync()
        {
            // Arrange
            var request = new TestArrayRequest();
            SetupHttpResponse(HttpStatusCode.OK, "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Length);
            Assert.Equal(1, result.Data[0].Id);
            Assert.Equal("A", result.Data[0].Name);
            Assert.Equal(2, result.Data[1].Id);
            Assert.Equal("B", result.Data[1].Name);
        }

        [Fact]
        public async Task SendAsync_WithIListResponse_DeserializesCorrectlyAsync()
        {
            // Arrange
            var request = new TestIListRequest();
            SetupHttpResponse(HttpStatusCode.OK, "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(1, result.Data[0].Id);
            Assert.Equal("A", result.Data[0].Name);
            Assert.Equal(2, result.Data[1].Id);
            Assert.Equal("B", result.Data[1].Name);
        }

        [Fact]
        public async Task SendAsync_WithICollectionResponse_DeserializesCorrectlyAsync()
        {
            // Arrange
            var request = new TestICollectionRequest();
            SetupHttpResponse(HttpStatusCode.OK, "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            var items = result.Data.ToArray();
            Assert.Equal(1, items[0].Id);
            Assert.Equal("A", items[0].Name);
            Assert.Equal(2, items[1].Id);
            Assert.Equal("B", items[1].Name);
        }

        [Fact]
        public async Task SendAsync_WithDeserializerTypeThatDoesNotImplementInterface_ThrowsInvalidOperationExceptionAsync()
        {
            // Arrange — register a type that is NOT IResponseDeserializer<TestResponse>
            _settings.AddDeserializer(typeof(WrongDeserializerRequest), typeof(NotADeserializer));
            var request = new WrongDeserializerRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _httpApiClient.SendAsync(request));

            Assert.Contains("does not implement IResponseDeserializer", ex.Message);
            Assert.Contains(nameof(TestResponse), ex.Message);
        }

        [Request("http://localhost/test")]
        private class WrongDeserializerRequest : IRequestResponse<TestResponse> { }

        // A class that does NOT implement IResponseDeserializer<TestResponse>
        private class NotADeserializer { }
    }
}
