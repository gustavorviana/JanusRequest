using JanusRequest.HttpHandlers;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientLoggingTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendAsync_Success_LogsRequestAndResponse_NoErrorAsync()
        {
            // Arrange
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");
            var logger = new TestLogger();
            _httpApiClient.Logger = logger;

            // Act
            var result = await _httpApiClient.GetAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal(1, logger.RequestCount);
            Assert.Equal(1, logger.ResponseCount);
            Assert.Equal(0, logger.ErrorCount);
            Assert.NotNull(logger.LastRequest);
            Assert.NotNull(logger.LastResponse);
            Assert.Equal(HttpMethod.Get, logger.LastRequest!.Method);
        }

        [Fact]
        public async Task SendAsync_WithHttpErrorHandler_LogsErrorWithRequestExceptionAndHeadersAsync()
        {
            // Arrange
            var request = new TestRequest();
            _settings.SetHandlers(new HttpErrorHandler());
            SetupHttpResponse(HttpStatusCode.BadRequest, "Bad Request");
            var logger = new TestLogger();
            _httpApiClient.Logger = logger;

            // Act
            var ex = await Assert.ThrowsAsync<RequestException>(() => _httpApiClient.SendAsync(request));

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            Assert.Equal(1, logger.ErrorCount);
            Assert.IsType<RequestException>(logger.LastException);

            var logged = (RequestException)logger.LastException!;
            Assert.Equal(HttpStatusCode.BadRequest, logged.StatusCode);
            // Headers should be captured by HttpErrorHandler
            Assert.NotNull(logged.Headers);
        }

        [Fact]
        public async Task SendRequestAsync_NonSuccessWithoutHandler_LogsErrorButDoesNotThrowAsync()
        {
            // Arrange
            var body = new { Id = 1, Name = "Test" };
            // No handlers registered in _settings, so HttpErrorHandler is not used
            SetupHttpResponse(HttpStatusCode.InternalServerError, "");
            var logger = new TestLogger();
            _httpApiClient.Logger = logger;

            // Act
            var result = await _httpApiClient.SendRequestAsync(body);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
            Assert.Equal(1, logger.ErrorCount);
            Assert.IsType<RequestException>(logger.LastException);
            Assert.Equal(HttpStatusCode.InternalServerError, ((RequestException)logger.LastException!).StatusCode);
        }
    }
}
