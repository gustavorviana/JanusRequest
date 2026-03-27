using NSubstitute;

namespace JanusRequest.Tests
{
    public class HttpApiClientConstructorTests : HttpApiClientTestBase
    {
        [Fact]
        public void Constructor_WithUrl_SetsUrlProperty()
        {
            // Arrange & Act
            var client = new HttpApiClient("https://api.example.com");

            // Assert
            Assert.Equal("https://api.example.com", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithHttpClient_SetsUrlFromBaseAddress()
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri("https://api.example.com") };

            // Act
            var client = new HttpApiClient(httpClient);

            // Assert
            Assert.Equal("https://api.example.com/", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithUrlAndHandler_CreatesClientWithCustomHandler()
        {
            // Arrange
            var handler = new MockHttpMessageHandler();

            // Act
            var client = new HttpApiClient("https://api.example.com", handler);

            // Assert
            Assert.Equal("https://api.example.com", client.Url);
            client.Dispose();
        }

        [Fact]
        public void Constructor_WithHandler_ThrowsWhenHandlerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpApiClient("https://api.example.com", (HttpMessageHandler)null!));
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new HttpApiClient((HttpClient)null!));
        }

        [Fact]
        public void Dispose_WithDisposeHttpClientTrue_DisposesHttpClient()
        {
            // Arrange
            var mockHttpClient = Substitute.For<HttpClient>();
            var client = new HttpApiClient(mockHttpClient, true);

            // Act
            client.Dispose();

            // Assert
            mockHttpClient.Received(1).Dispose();
        }

        [Fact]
        public void Dispose_WithDisposeHttpClientFalse_DoesNotDisposeHttpClient()
        {
            // Arrange
            var mockHttpClient = Substitute.For<HttpClient>();
            var client = new HttpApiClient(mockHttpClient, false);

            // Act
            client.Dispose();

            // Assert
            mockHttpClient.DidNotReceive().Dispose();
        }
    }
}
