using JanusRequest.HttpHandlers;
using System.Net;
using System.Text;

namespace JanusRequest.Tests.HttpHandlers
{
    public class HttpErrorHandlerTests
    {
        private readonly HttpErrorHandler _handler;
        private readonly HttpApiClientSettings _settings;

        public HttpErrorHandlerTests()
        {
            _handler = new HttpErrorHandler();
            _settings = new HttpApiClientSettings();
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
        [InlineData(HttpStatusCode.MovedPermanently, false)]
        [InlineData(HttpStatusCode.Redirect, false)]
        [InlineData(HttpStatusCode.NotModified, false)]
        public void CanHandle_WithDifferentStatusCodes_ShouldReturnExpectedResult(HttpStatusCode statusCode, bool expected)
        {
            var response = new HttpResponseMessage(statusCode);

            var result = _handler.CanHandle(response);

            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task MapExceptionAsync_WithUnauthorizedStatus_ShouldReturnRequestException()
        {
            var response = CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized");

            var result = await _handler.MapExceptionAsync(response, _settings);

            Assert.IsType<RequestException>(result);
            Assert.Equal(HttpStatusCode.Unauthorized, ((RequestException)result).StatusCode);
        }

        [Fact]
        public async Task MapExceptionAsync_WithThrottlingStatus_ShouldReturnThrottlingException()
        {
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");
            response.Headers.Add("Retry-After", "60");

            var result = await _handler.MapExceptionAsync(response, _settings);

            Assert.IsType<ThrottlingException>(result);
        }

        [Fact]
        public async Task MapExceptionAsync_WithThrottlingStatusAndNoRetryAfterHeader_ShouldReturnThrottlingExceptionWithZeroRetry()
        {
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");

            var result = await _handler.MapExceptionAsync(response, _settings);

            var throttlingException = Assert.IsType<ThrottlingException>(result);
            Assert.Equal(0, throttlingException.RetryAfter);
        }

        [Fact]
        public async Task MapExceptionAsync_WithOtherErrorStatus_ShouldReturnRequestException()
        {
            var responseContent = "InternalServerError";
            var requestUri = new Uri("https://api.example.com/test");
            var response = CreateResponse(HttpStatusCode.InternalServerError, responseContent);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var result = await _handler.MapExceptionAsync(response, _settings);

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
            var response = CreateResponse(statusCode, "Error message");

            var result = await _handler.MapExceptionAsync(response, _settings);

            Assert.IsType<RequestException>(result);
            Assert.Equal(statusCode, ((RequestException)result).StatusCode);
        }

        [Fact]
        public async Task OnThrottling_ShouldReturnThrottlingExceptionWithCorrectRetryAfter()
        {
            var response = CreateResponse((HttpStatusCode)429, "Too Many Requests");
            response.Headers.Add("Retry-After", "300");

            var result = await _handler.MapExceptionAsync(response, _settings);

            var throttlingException = Assert.IsType<ThrottlingException>(result);
            Assert.Equal(300, throttlingException.RetryAfter);
            Assert.Equal(0, throttlingException.RequestLimit);
        }

        [Fact]
        public async Task MapExceptionAsync_WithNullContent_DoesNotThrow()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = null!,
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test")
            };

            var result = await _handler.MapExceptionAsync(response, _settings);

            Assert.IsType<RequestException>(result);
            var requestException = (RequestException)result;
            Assert.Equal(HttpStatusCode.InternalServerError, requestException.StatusCode);
        }

        [Fact]
        public async Task MapExceptionAsync_WithEmptyContent_ReturnsRequestExceptionWithEmptyMessage()
        {
            var response = CreateResponse(HttpStatusCode.BadRequest, "");

            var result = await _handler.MapExceptionAsync(response, _settings);

            Assert.IsType<RequestException>(result);
        }

        [Fact]
        public async Task MapExceptionAsync_WithProblemDetailsBody_ReturnsProblemDetailsException()
        {
            var problemBody = "{\"type\":\"https://example.com/probs/out-of-credit\",\"title\":\"You do not have enough credit.\",\"status\":400,\"detail\":\"Your current balance is 30, but that costs 50.\"}";
            var response = CreateResponse(HttpStatusCode.BadRequest, problemBody, "application/problem+json");

            var result = await _handler.MapExceptionAsync(response, _settings);

            var problemException = Assert.IsType<ProblemDetailsException>(result);
            Assert.Equal(HttpStatusCode.BadRequest, problemException.StatusCode);
            Assert.Equal("You do not have enough credit.", problemException.Title);
            Assert.Equal("Your current balance is 30, but that costs 50.", problemException.Detail);
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string content, string mediaType = "application/json")
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(content, Encoding.UTF8, mediaType);
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            return response;
        }
    }
}
