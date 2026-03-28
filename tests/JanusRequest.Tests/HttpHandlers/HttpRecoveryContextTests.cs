using JanusRequest.HttpHandlers;
using NSubstitute;
using System.Net;
using System.Text;

namespace JanusRequest.Tests.HttpHandlers
{
    public class HttpRecoveryContextTests
    {
        [Fact]
        public async Task CloneRequestAsync_PreservesMethod()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api");

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.Equal(HttpMethod.Post, clone.Method);
        }

        [Fact]
        public async Task CloneRequestAsync_PreservesRequestUri()
        {
            // Arrange
            var uri = new Uri("https://example.com/api/resource?key=value");
            var original = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.Equal(uri, clone.RequestUri);
        }

        [Fact]
        public async Task CloneRequestAsync_PreservesVersion()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Get, "https://example.com")
            {
                Version = new Version(2, 0)
            };

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.Equal(new Version(2, 0), clone.Version);
        }

        [Fact]
        public async Task CloneRequestAsync_PreservesHeaders()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            original.Headers.Add("X-Custom-Header", "custom-value");
            original.Headers.Add("Accept", "application/json");

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.Contains("custom-value", clone.Headers.GetValues("X-Custom-Header"));
            Assert.Contains("application/json", clone.Headers.GetValues("Accept"));
        }

        [Fact]
        public async Task CloneRequestAsync_PreservesContent()
        {
            // Arrange
            var body = "{\"name\":\"test\"}";
            var original = new HttpRequestMessage(HttpMethod.Post, "https://example.com")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.NotNull(clone.Content);
            var clonedBody = await clone.Content.ReadAsStringAsync();
            Assert.Equal(body, clonedBody);
        }

        [Fact]
        public async Task CloneRequestAsync_PreservesContentHeaders()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Post, "https://example.com")
            {
                Content = new StringContent("data", Encoding.UTF8, "text/plain")
            };

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.NotNull(clone.Content);
            Assert.Equal("text/plain", clone.Content.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task CloneRequestAsync_WithNullContent_ClonesWithoutContent()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.Null(clone.Content);
        }

        [Fact]
        public async Task CloneRequestAsync_ReturnsNewInstance()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Get, "https://example.com");

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.NotSame(original, clone);
        }

        [Fact]
        public async Task CloneRequestAsync_ClonedContentIsIndependent()
        {
            // Arrange
            var original = new HttpRequestMessage(HttpMethod.Post, "https://example.com")
            {
                Content = new StringContent("original-data")
            };

            // Act
            var clone = await HttpRecoveryContext.CloneRequestAsync(original);

            // Assert
            Assert.NotSame(original.Content, clone.Content);
        }

        [Fact]
        public async Task ResendAsync_SendsClonedRequest()
        {
            // Arrange
            var handler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("success")
            };

            handler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(expectedResponse));

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };
            var originalRequest = new HttpRequestMessage(HttpMethod.Get, "https://example.com/test");
            var originalResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

            var context = new HttpRecoveryContext(httpClient, originalRequest, originalResponse, CancellationToken.None);

            // Act
            var result = await context.ResendAsync();

            // Assert
            Assert.Same(expectedResponse, result);
            await handler.Received(1).OnSendedAsync(
                Arg.Is<HttpRequestMessage>(r => r != originalRequest && r.Method == HttpMethod.Get),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ResendAsync_UsesClonedRequestNotOriginal()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var handler = Substitute.For<HttpApiClientTestBase.MockHttpMessageHandler>();

            handler.OnSendedAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    capturedRequest = callInfo.ArgAt<HttpRequestMessage>(0);
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                });

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.com") };
            var originalRequest = new HttpRequestMessage(HttpMethod.Post, "https://example.com/test")
            {
                Content = new StringContent("body-data")
            };
            var originalResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            var context = new HttpRecoveryContext(httpClient, originalRequest, originalResponse, CancellationToken.None);

            // Act
            await context.ResendAsync();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.NotSame(originalRequest, capturedRequest);
            Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        }
    }
}
