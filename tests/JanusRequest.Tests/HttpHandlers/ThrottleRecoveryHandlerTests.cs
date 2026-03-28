using JanusRequest.HttpHandlers;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests.HttpHandlers
{
    public class ThrottleRecoveryHandlerTests
    {
        [Fact]
        public void CanHandle_With429StatusCode_ReturnsTrue()
        {
            // Arrange
            var handler = new ThrottleRecoveryHandler();
            var response = new HttpResponseMessage((HttpStatusCode)429);

            // Act
            var result = handler.CanHandle(response);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanHandle_WithNon429StatusCode_ReturnsFalse()
        {
            // Arrange
            var handler = new ThrottleRecoveryHandler();
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // Act
            var result = handler.CanHandle(response);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RecoverAsync_DisposesOriginalResponse()
        {
            // Arrange
            var handler = new ThrottleRecoveryHandler();
            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();
            var recoveredResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("success")
            };

            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(recoveredResponse));

            var httpClient = new HttpClient(mockHandler);
            var originalRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var originalResponse = new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent("throttled")
            };
            originalResponse.Headers.Add("Retry-After", "0");

            var context = new HttpRecoveryContext(httpClient, originalRequest, originalResponse, CancellationToken.None);

            // Act
            var result = await handler.RecoverAsync(context);

            // Assert
            Assert.Same(recoveredResponse, result);

            // Verify original response was disposed by trying to read content (should throw)
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await originalResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task RecoverAsync_UsesResendAsync_SendsClonedRequest()
        {
            // Arrange
            var handler = new ThrottleRecoveryHandler();
            HttpRequestMessage? capturedRequest = null;
            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();

            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    capturedRequest = callInfo.ArgAt<HttpRequestMessage>(0);
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("recovered")
                    });
                });

            var httpClient = new HttpClient(mockHandler);
            var originalRequest = new HttpRequestMessage(HttpMethod.Post, "https://example.com/test")
            {
                Content = new StringContent("body")
            };
            var originalResponse = new HttpResponseMessage((HttpStatusCode)429);
            originalResponse.Headers.Add("Retry-After", "0");

            var context = new HttpRecoveryContext(httpClient, originalRequest, originalResponse, CancellationToken.None);

            // Act
            await handler.RecoverAsync(context);

            // Assert - request was cloned (not the same instance)
            Assert.NotNull(capturedRequest);
            Assert.NotSame(originalRequest, capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        }

        [Fact]
        public async Task RecoverAsync_RespectsRetryAfterHeader()
        {
            // Arrange
            var handler = new ThrottleRecoveryHandler();
            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();

            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var httpClient = new HttpClient(mockHandler);
            var originalRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var originalResponse = new HttpResponseMessage((HttpStatusCode)429);
            originalResponse.Headers.Add("Retry-After", "1");

            var context = new HttpRecoveryContext(httpClient, originalRequest, originalResponse, CancellationToken.None);

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await handler.RecoverAsync(context);
            stopwatch.Stop();

            // Assert - should have waited at least ~1 second
            Assert.True(stopwatch.ElapsedMilliseconds >= 900, $"Expected at least 900ms delay, got {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
