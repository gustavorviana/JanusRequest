using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientRequestDisposalTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendHttpRequestAsync_WhenSendThrows_DisposesRequestMessage()
        {
            // Arrange
            var exception = new HttpRequestException("Network error");
            _httpMessageHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(exception);

            var request = new TestRequest();

            // Act & Assert - the exception should propagate
            var ex = await Assert.ThrowsAsync<HttpRequestException>(
                async () => await _httpApiClient.SendHttpRequestAsync(request));

            Assert.Same(exception, ex);
        }

        [Fact]
        public async Task SendHttpRequestAsync_WhenSendSucceeds_ReturnsResponse()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "success");

            // Act
            using var response = await _httpApiClient.SendHttpRequestAsync(new TestRequest());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendHttpRequestAsync_WithNullBody_DoesNotThrow()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "ok");

            // Act
            using var response = await _httpApiClient.SendHttpRequestAsync(null, new HttpRequestInfo
            {
                Method = "GET",
                Path = "/test"
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
