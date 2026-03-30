using JanusRequest.HttpHandlers;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests.HttpHandlers
{
    public class ThrottleRetryHandlerTests
    {
        #region Constructor validation

        [Fact]
        public void Constructor_Default_UsesDefaultValues()
        {
            var handler = new ThrottleRetryHandler();

            Assert.Equal(3, handler.MaxRetries);
            Assert.Equal(1.0, handler.BaseDelaySeconds);
            Assert.Equal(60.0, handler.MaxDelaySeconds);
            Assert.Equal(RetryDelayStrategy.ExponentialBackoff, handler.DelayStrategy);
        }

        [Fact]
        public void Constructor_WithParameters_SetsValues()
        {
            var handler = new ThrottleRetryHandler(5, 2.0, 120.0, RetryDelayStrategy.Jitter);

            Assert.Equal(5, handler.MaxRetries);
            Assert.Equal(2.0, handler.BaseDelaySeconds);
            Assert.Equal(120.0, handler.MaxDelaySeconds);
            Assert.Equal(RetryDelayStrategy.Jitter, handler.DelayStrategy);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidMaxRetries_Throws(int maxRetries)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ThrottleRetryHandler(maxRetries, 1.0, 60.0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidBaseDelay_Throws(double baseDelay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ThrottleRetryHandler(3, baseDelay, 60.0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_InvalidMaxDelay_Throws(double maxDelay)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ThrottleRetryHandler(3, 1.0, maxDelay));
        }

        [Fact]
        public void MaxDelay_ReturnsTimeSpan()
        {
            var handler = new ThrottleRetryHandler(3, 1.0, 30.0);
            Assert.Equal(TimeSpan.FromSeconds(30), handler.MaxDelay);
        }

        #endregion

        #region CanHandle — status codes

        [Theory]
        [InlineData(408, true)]  // Request Timeout
        [InlineData(429, true)]  // Too Many Requests
        [InlineData(503, true)]  // Service Unavailable
        [InlineData(504, true)]  // Gateway Timeout
        [InlineData(200, false)] // OK
        [InlineData(400, false)] // Bad Request
        [InlineData(401, false)] // Unauthorized
        [InlineData(404, false)] // Not Found
        [InlineData(500, false)] // Internal Server Error
        [InlineData(502, false)] // Bad Gateway
        public void CanHandle_DefaultStatusCodes(int statusCode, bool expected)
        {
            var handler = new ThrottleRetryHandler();
            var response = new HttpResponseMessage((HttpStatusCode)statusCode);

            Assert.Equal(expected, handler.CanHandle(response));
        }

        [Fact]
        public void CanHandle_CustomStatusCodes_OnlyMatchesConfigured()
        {
            var handler = new ThrottleRetryHandler(3, 1.0, 60.0, RetryDelayStrategy.ExponentialBackoff,
                new[] { 500, 502 });

            Assert.True(handler.CanHandle(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
            Assert.True(handler.CanHandle(new HttpResponseMessage((HttpStatusCode)502)));
            Assert.False(handler.CanHandle(new HttpResponseMessage((HttpStatusCode)429)));
        }

        #endregion

        #region TryParseRetryAfter — RFC 9110

        [Fact]
        public void TryParseRetryAfter_DelaySeconds_ParsesCorrectly()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", "120");

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.True(result);
            Assert.Equal(TimeSpan.FromSeconds(120), delay);
        }

        [Fact]
        public void TryParseRetryAfter_ZeroSeconds_ParsesAsZero()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", "0");

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.True(result);
            Assert.Equal(TimeSpan.Zero, delay);
        }

        [Fact]
        public void TryParseRetryAfter_HttpDate_ParsesCorrectly()
        {
            var futureDate = DateTime.UtcNow.AddSeconds(60);
            var httpDate = futureDate.ToString("r");

            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", httpDate);

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.True(result);
            Assert.True(delay.TotalSeconds > 50 && delay.TotalSeconds <= 61,
                $"Expected delay around 60s, got {delay.TotalSeconds}s");
        }

        [Fact]
        public void TryParseRetryAfter_PastHttpDate_ReturnsZeroDelay()
        {
            var pastDate = DateTime.UtcNow.AddSeconds(-60);
            var httpDate = pastDate.ToString("r");

            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", httpDate);

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.True(result);
            Assert.Equal(TimeSpan.Zero, delay);
        }

        [Fact]
        public void TryParseRetryAfter_NoHeader_ReturnsFalse()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.False(result);
            Assert.Equal(TimeSpan.Zero, delay);
        }

        [Fact]
        public void TryParseRetryAfter_InvalidValue_ReturnsFalse()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.TryAddWithoutValidation("Retry-After", "not-a-number-or-date");

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.False(result);
        }

        [Fact]
        public void TryParseRetryAfter_EmptyValue_ReturnsFalse()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.TryAddWithoutValidation("Retry-After", "");

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.False(result);
        }

        [Fact]
        public void TryParseRetryAfter_LargeSeconds_ParsesCorrectly()
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", "3600");

            var result = ThrottleRetryHandler.TryParseRetryAfter(response, out var delay);

            Assert.True(result);
            Assert.Equal(TimeSpan.FromHours(1), delay);
        }

        #endregion

        #region ComputeDelay — ExponentialBackoff

        [Theory]
        [InlineData(1, 1.0)]  // 1 * 2^0 = 1
        [InlineData(2, 2.0)]  // 1 * 2^1 = 2
        [InlineData(3, 4.0)]  // 1 * 2^2 = 4
        [InlineData(4, 8.0)]  // 1 * 2^3 = 8
        [InlineData(5, 16.0)] // 1 * 2^4 = 16
        public void ComputeDelay_ExponentialBackoff_NoRetryAfter_CorrectProgression(int attempt, double expectedSeconds)
        {
            var handler = new ThrottleRetryHandler(5, 1.0, 120.0, RetryDelayStrategy.ExponentialBackoff);
            var response = new HttpResponseMessage((HttpStatusCode)429);

            var delay = handler.ComputeDelay(response, attempt);

            Assert.Equal(expectedSeconds, delay.TotalSeconds, precision: 10);
        }

        [Fact]
        public void ComputeDelay_ExponentialBackoff_WithCustomBaseDelay()
        {
            var handler = new ThrottleRetryHandler(3, 2.0, 120.0, RetryDelayStrategy.ExponentialBackoff);
            var response = new HttpResponseMessage((HttpStatusCode)429);

            Assert.Equal(2.0, handler.ComputeDelay(response, 1).TotalSeconds, precision: 10);  // 2 * 2^0
            Assert.Equal(4.0, handler.ComputeDelay(response, 2).TotalSeconds, precision: 10);  // 2 * 2^1
            Assert.Equal(8.0, handler.ComputeDelay(response, 3).TotalSeconds, precision: 10);  // 2 * 2^2
        }

        [Fact]
        public void ComputeDelay_ExponentialBackoff_CappedAtMaxDelay()
        {
            var handler = new ThrottleRetryHandler(10, 1.0, 10.0, RetryDelayStrategy.ExponentialBackoff);
            var response = new HttpResponseMessage((HttpStatusCode)429);

            // attempt 5 = 1 * 2^4 = 16, but capped at 10
            var delay = handler.ComputeDelay(response, 5);

            Assert.Equal(10.0, delay.TotalSeconds, precision: 10);
        }

        [Fact]
        public void ComputeDelay_ExponentialBackoff_WithRetryAfterHeader_UsesHeaderDirectly()
        {
            var handler = new ThrottleRetryHandler(3, 1.0, 120.0, RetryDelayStrategy.ExponentialBackoff);
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", "5");

            // Retry-After present: use value directly, no exponential applied
            Assert.Equal(5.0, handler.ComputeDelay(response, 1).TotalSeconds, precision: 10);
            Assert.Equal(5.0, handler.ComputeDelay(response, 2).TotalSeconds, precision: 10);
            Assert.Equal(5.0, handler.ComputeDelay(response, 3).TotalSeconds, precision: 10);
        }

        #endregion

        #region ComputeDelay — Jitter

        [Fact]
        public void ComputeDelay_Jitter_AlwaysInExpectedRange()
        {
            // Use real Random to verify range over many iterations
            var handler = new ThrottleRetryHandler(3, 10.0, 120.0, RetryDelayStrategy.Jitter);
            var response = new HttpResponseMessage((HttpStatusCode)429);

            for (int i = 0; i < 100; i++)
            {
                var delay = handler.ComputeDelay(response, 1);

                // base=10, jitter=[0.5,1.5), so range is [5.0, 15.0)
                Assert.True(delay.TotalSeconds >= 5.0,
                    $"Delay {delay.TotalSeconds}s is below minimum 5.0s");
                Assert.True(delay.TotalSeconds < 15.0,
                    $"Delay {delay.TotalSeconds}s is at or above maximum 15.0s");
            }
        }

        [Fact]
        public void ComputeDelay_Jitter_WithDeterministicRandom_ProducesExpectedResult()
        {
            // Seed Random for deterministic output
            var deterministicRandom = new Random(42);
            var handler = new ThrottleRetryHandler(3, 10.0, 120.0, RetryDelayStrategy.Jitter,
                new[] { 429 }, deterministicRandom);

            var response = new HttpResponseMessage((HttpStatusCode)429);
            var delay = handler.ComputeDelay(response, 1);

            // With Random(42), NextDouble() = ~0.7396... so jitter = 0.5 + 0.7396 = 1.2396
            // delay = 10 * 1.2396 = ~12.396
            Assert.True(delay.TotalSeconds >= 5.0 && delay.TotalSeconds < 15.0,
                $"Expected delay in [5, 15), got {delay.TotalSeconds}");
        }

        [Fact]
        public void ComputeDelay_Jitter_ScalesWithAttemptViaExponentialBackoff()
        {
            // Jitter applies on top of exponential backoff: base * 2^(attempt-1) * jitter
            var deterministicRandom = new Random(42);
            var handler = new ThrottleRetryHandler(3, 10.0, 1000.0, RetryDelayStrategy.Jitter,
                new[] { 429 }, deterministicRandom);

            var response = new HttpResponseMessage((HttpStatusCode)429);

            var delay1 = handler.ComputeDelay(response, 1); // 10 * 1 * jitter -> [5, 15)
            var delay2 = handler.ComputeDelay(response, 2); // 10 * 2 * jitter -> [10, 30)
            var delay3 = handler.ComputeDelay(response, 3); // 10 * 4 * jitter -> [20, 60)

            Assert.True(delay1.TotalSeconds >= 5.0 && delay1.TotalSeconds < 15.0);
            Assert.True(delay2.TotalSeconds >= 10.0 && delay2.TotalSeconds < 30.0);
            Assert.True(delay3.TotalSeconds >= 20.0 && delay3.TotalSeconds < 60.0);
        }

        [Fact]
        public void ComputeDelay_Jitter_WithRetryAfterHeader_UsesHeaderDirectly()
        {
            var deterministicRandom = new Random(42);
            var handler = new ThrottleRetryHandler(3, 1.0, 120.0, RetryDelayStrategy.Jitter,
                new[] { 429 }, deterministicRandom);

            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", "20");

            // Retry-After present: use value directly, no jitter applied
            Assert.Equal(20.0, handler.ComputeDelay(response, 1).TotalSeconds, precision: 10);
            Assert.Equal(20.0, handler.ComputeDelay(response, 2).TotalSeconds, precision: 10);
        }

        [Fact]
        public void ComputeDelay_Jitter_CappedAtMaxDelay()
        {
            // Force high base so jitter produces values above max
            var handler = new ThrottleRetryHandler(3, 100.0, 10.0, RetryDelayStrategy.Jitter);
            var response = new HttpResponseMessage((HttpStatusCode)429);

            var delay = handler.ComputeDelay(response, 1);

            Assert.True(delay.TotalSeconds <= 10.0,
                $"Expected delay <= 10.0s, got {delay.TotalSeconds}s");
        }

        #endregion

        #region RecoverAsync — retry loop

        [Fact]
        public async Task RecoverAsync_SuccessOnFirstRetry_ReturnsResponse()
        {
            var handler = CreateHandler(maxRetries: 3, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext(HttpStatusCode.ServiceUnavailable);

            // First retry returns OK
            SetupRetryResponses(mockHandler, HttpStatusCode.OK);

            var result = await handler.RecoverAsync(context);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RecoverAsync_SuccessAfterMultipleRetries_ReturnsResponse()
        {
            var handler = CreateHandler(maxRetries: 5, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext((HttpStatusCode)429);

            // Fail twice more, then succeed
            var callCount = 0;
            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    callCount++;
                    if (callCount <= 2)
                        return Task.FromResult(new HttpResponseMessage((HttpStatusCode)429));

                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("success")
                    });
                });

            var result = await handler.RecoverAsync(context);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(3, callCount);
        }

        [Fact]
        public async Task RecoverAsync_ExhaustsRetries_ReturnsLastErrorResponse()
        {
            var handler = CreateHandler(maxRetries: 2, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext((HttpStatusCode)429);

            // Always return 429
            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromResult(new HttpResponseMessage((HttpStatusCode)429)));

            var result = await handler.RecoverAsync(context);

            Assert.Equal((HttpStatusCode)429, result.StatusCode);
        }

        [Fact]
        public async Task RecoverAsync_DisposesIntermediateResponses()
        {
            var handler = CreateHandler(maxRetries: 3, baseDelay: 0.001, maxDelay: 0.01);
            var intermediateResponse = new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent("throttled-intermediate")
            };

            var (context, mockHandler) = CreateMockContext((HttpStatusCode)429);

            var callCount = 0;
            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    callCount++;
                    if (callCount == 1)
                        return Task.FromResult(intermediateResponse);

                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            await handler.RecoverAsync(context);

            // Verify intermediate response was disposed
            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await intermediateResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task RecoverAsync_DisposesOriginalResponse()
        {
            var handler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var originalResponse = new HttpResponseMessage((HttpStatusCode)503)
            {
                Content = new StringContent("service-unavailable")
            };

            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();
            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var context = new HttpRecoveryContext(httpClient, request, originalResponse, CancellationToken.None);

            await handler.RecoverAsync(context);

            await Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await originalResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task RecoverAsync_Handles408RequestTimeout()
        {
            var handler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext(HttpStatusCode.RequestTimeout);

            SetupRetryResponses(mockHandler, HttpStatusCode.OK);

            var result = await handler.RecoverAsync(context);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RecoverAsync_Handles503ServiceUnavailable()
        {
            var handler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext(HttpStatusCode.ServiceUnavailable);

            SetupRetryResponses(mockHandler, HttpStatusCode.OK);

            var result = await handler.RecoverAsync(context);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RecoverAsync_Handles504GatewayTimeout()
        {
            var handler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var (context, mockHandler) = CreateMockContext(HttpStatusCode.GatewayTimeout);

            SetupRetryResponses(mockHandler, HttpStatusCode.OK);

            var result = await handler.RecoverAsync(context);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RecoverAsync_CancellationToken_IsPropagated()
        {
            var handler = CreateHandler(maxRetries: 3, baseDelay: 5.0, maxDelay: 60.0);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();
            var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var response = new HttpResponseMessage((HttpStatusCode)429);

            var context = new HttpRecoveryContext(httpClient, request, response, cts.Token);

            await Assert.ThrowsAsync<TaskCanceledException>(
                async () => await handler.RecoverAsync(context));
        }

        #endregion

        #region Integration with HttpApiClient

        [Fact]
        public async Task Handler_IntegratesWithSettings()
        {
            var handler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var settings = new HttpApiClientSettings();
            settings.SetHandlers(handler);

            Assert.True(settings.TryGetHandler<IHttpRecoveryHandler>(
                new HttpResponseMessage((HttpStatusCode)429), out var resolved));
            Assert.Same(handler, resolved);
        }

        [Fact]
        public async Task Handler_DoesNotConflictWithErrorHandler()
        {
            var retryHandler = CreateHandler(maxRetries: 1, baseDelay: 0.001, maxDelay: 0.01);
            var errorHandler = new HttpErrorHandler();
            var settings = new HttpApiClientSettings();
            settings.SetHandlers(retryHandler, errorHandler);

            // Recovery handler should be found for 429
            Assert.True(settings.TryGetHandler<IHttpRecoveryHandler>(
                new HttpResponseMessage((HttpStatusCode)429), out _));

            // Error handler should be found for 400
            Assert.True(settings.TryGetHandler<HttpErrorHandler>(
                new HttpResponseMessage(HttpStatusCode.BadRequest), out _));
        }

        #endregion

        #region Helpers

        private static ThrottleRetryHandler CreateHandler(int maxRetries = 3, double baseDelay = 1.0, double maxDelay = 60.0,
            RetryDelayStrategy strategy = RetryDelayStrategy.ExponentialBackoff)
        {
            return new ThrottleRetryHandler(maxRetries, baseDelay, maxDelay, strategy);
        }

        private static (HttpRecoveryContext context, HttpApiClientTestBase.MockHttpMessageHandler mockHandler)
            CreateMockContext(HttpStatusCode initialStatusCode)
        {
            var mockHandler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();
            var httpClient = new HttpClient(mockHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var response = new HttpResponseMessage(initialStatusCode);
            var context = new HttpRecoveryContext(httpClient, request, response, CancellationToken.None);

            return (context, mockHandler);
        }

        private static void SetupRetryResponses(HttpApiClientTestBase.MockHttpMessageHandler mockHandler, params HttpStatusCode[] statusCodes)
        {
            var index = 0;
            mockHandler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    var code = statusCodes[Math.Min(index, statusCodes.Length - 1)];
                    index++;
                    return Task.FromResult(new HttpResponseMessage(code));
                });
        }

        #endregion
    }
}
