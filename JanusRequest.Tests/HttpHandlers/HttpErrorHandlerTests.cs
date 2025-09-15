using JanusRequest.HttpHandlers;
using System.Net;
using System.Text;

namespace JanusRequest.Tests.HttpHandlers
{
    public class HttpErrorHandlerTests
    {
        private readonly HttpErrorHandler _handler;

        public HttpErrorHandlerTests()
        {
            _handler = new HttpErrorHandler();
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, true)]
        [InlineData(HttpStatusCode.Unauthorized, true)]
        [InlineData(HttpStatusCode.Forbidden, true)]
        [InlineData(HttpStatusCode.NotFound, true)]
        [InlineData(HttpStatusCode.InternalServerError, true)]
        [InlineData(HttpStatusCode.OK, false)]
        [InlineData(HttpStatusCode.Created, false)]
        [InlineData(HttpStatusCode.NoContent, false)]
        public void CanHandle_WithDifferentStatusCodes_ShouldReturnExpectedResult(HttpStatusCode statusCode, bool expected)
        {
            // Arrange
            var response = new HttpResponseMessage(statusCode);

            // Act
            var result = _handler.CanHandle(response);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CanHandle_WithSuccessStatusCode_ShouldReturnFalse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            var result = _handler.CanHandle(response);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MapExceptionAsync_WithUnauthorizedStatus_ShouldReturnUnauthorizedAccessException()
        {
            // Arrange
            var response = CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized");

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            Assert.IsType<UnauthorizedAccessException>(result);
            Assert.Equal("O servidor recusou as credenciais da API.", result.Message);
        }

        [Fact]
        public async Task MapExceptionAsync_WithThrottlingStatus_ShouldReturnThrottlingException()
        {
            // Arrange
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");
            response.Headers.Add("Retry-After", "60");

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            Assert.IsType<ThrottlingException>(result);
        }

        [Fact]
        public async Task MapExceptionAsync_WithThrottlingStatusAndNoRetryAfterHeader_ShouldReturnThrottlingExceptionWithZeroRetry()
        {
            // Arrange
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            var throttlingException = Assert.IsType<ThrottlingException>(result);
            Assert.Equal(0, throttlingException.RetryAfter);
        }

        [Fact]
        public async Task MapExceptionAsync_WithOtherErrorStatus_ShouldReturnRequestException()
        {
            // Arrange
            var responseContent = "InternalServerError";
            var requestUri = new Uri("https://api.example.com/test");
            var response = CreateResponse(HttpStatusCode.InternalServerError, responseContent);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            Assert.IsType<RequestException>(result);
            var requestException = (RequestException)result;
            Assert.Equal(HttpStatusCode.InternalServerError, requestException.StatusCode);
            Assert.Equal(requestUri.ToString(), requestException.Url);
            Assert.Equal("Error code: " + responseContent, requestException.Message);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task MapExceptionAsync_WithVariousErrorCodes_ShouldReturnRequestException(HttpStatusCode statusCode)
        {
            // Arrange
            var response = CreateResponse(statusCode, "Error message");

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            Assert.IsType<RequestException>(result);
            Assert.Equal(statusCode, ((RequestException)result).StatusCode);
        }

        [Fact]
        public async Task OnThrottling_ShouldReturnThrottlingExceptionWithCorrectRetryAfter()
        {
            // Arrange
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");
            response.Headers.Add("Retry-After", "300");

            // Act
            var result = await _handler.MapExceptionAsync(response);

            // Assert
            var throttlingException = Assert.IsType<ThrottlingException>(result);
            Assert.Equal(300, throttlingException.RetryAfter);
            Assert.Equal(0, throttlingException.RequestLimit);
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(content, Encoding.UTF8, "application/json");
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            return response;
        }
    }
}