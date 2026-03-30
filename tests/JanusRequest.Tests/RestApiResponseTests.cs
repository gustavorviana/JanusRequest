using NSubstitute;
using System.Net;

namespace JanusRequest.Tests
{
    public class RestApiResponseTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task GetHeader_CaseInsensitive_ReturnsValue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":1,\"Name\":\"Test\"}")
            };
            response.Headers.Add("X-Custom", "value");

            _httpMessageHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(response));

            // Act
            var result = await _httpApiClient.GetAsync(new TestRequest());

            // Assert
            Assert.Equal("value", result.GetHeader("x-custom"));
        }

        [Fact]
        public async Task HasHeader_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":1,\"Name\":\"Test\"}")
            };
            response.Headers.Add("X-Custom", "value");

            _httpMessageHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(response));

            // Act
            var result = await _httpApiClient.GetAsync(new TestRequest());

            // Assert
            Assert.True(result.HasHeader("x-custom"));
        }

        [Fact]
        public async Task GetHeader_NonExistent_ReturnsNull()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":1,\"Name\":\"Test\"}")
            };

            _httpMessageHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(response));

            // Act
            var result = await _httpApiClient.GetAsync(new TestRequest());

            // Assert
            Assert.Null(result.GetHeader("missing"));
        }
    }
}
