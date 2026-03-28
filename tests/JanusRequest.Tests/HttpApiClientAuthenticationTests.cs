namespace JanusRequest.Tests
{
    public class HttpApiClientAuthenticationTests : HttpApiClientTestBase
    {
        [Fact]
        public void SetBasicAuthentication_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetBasicAuthentication("user", "password");

            // Assert
            Assert.Equal("Basic", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:password")),
                _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetBearerAuthentication_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetBearerAuthentication("token123");

            // Assert
            Assert.Equal("Bearer", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal("token123", _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetApiKeyAuthentication_SetsCustomHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123", "X-Custom-Key");

            // Assert
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-Custom-Key"));
            Assert.Contains("key123", _httpClient.DefaultRequestHeaders.GetValues("X-Custom-Key"));
        }

        [Fact]
        public void SetApiKeyAuthentication_WithDefaultHeader_SetsXApiKeyHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123");

            // Assert
            Assert.True(_httpClient.DefaultRequestHeaders.Contains("X-API-Key"));
            Assert.Contains("key123", _httpClient.DefaultRequestHeaders.GetValues("X-API-Key"));
        }

        [Fact]
        public void ClearAuthentication_RemovesAuthorizationHeader()
        {
            // Arrange
            _httpApiClient.SetBearerAuthentication("token");

            // Act
            _httpApiClient.ClearAuthentication();

            // Assert
            Assert.Null(_httpClient.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public void SetAuthentication_WithCustomScheme_SetsAuthorizationHeader()
        {
            // Act
            _httpApiClient.SetAuthentication("Custom", "custom-token");

            // Assert
            Assert.Equal("Custom", _httpClient.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal("custom-token", _httpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetBasicAuthentication_ReturnsIHttpApiClient()
        {
            // Act
            IHttpApiClient result = _httpApiClient.SetBasicAuthentication("user", "pass");

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
            Assert.Same(_httpApiClient, result);
        }

        [Fact]
        public void SetBearerAuthentication_ReturnsIHttpApiClient()
        {
            // Act
            IHttpApiClient result = _httpApiClient.SetBearerAuthentication("token");

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
            Assert.Same(_httpApiClient, result);
        }

        [Fact]
        public void SetApiKeyAuthentication_ReturnsIHttpApiClient()
        {
            // Act
            IHttpApiClient result = _httpApiClient.SetApiKeyAuthentication("key");

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
            Assert.Same(_httpApiClient, result);
        }

        [Fact]
        public void SetAuthentication_ReturnsIHttpApiClient()
        {
            // Act
            IHttpApiClient result = _httpApiClient.SetAuthentication("Custom", "value");

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
            Assert.Same(_httpApiClient, result);
        }

        [Fact]
        public void ClearAuthentication_ReturnsIHttpApiClient()
        {
            // Act
            IHttpApiClient result = _httpApiClient.ClearAuthentication();

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
            Assert.Same(_httpApiClient, result);
        }

        [Fact]
        public void AuthMethods_CanBeChainedThroughInterface()
        {
            // Act - chain all auth methods through IHttpApiClient interface
            IHttpApiClient client = _httpApiClient;
            var result = client
                .SetBearerAuthentication("token1")
                .ClearAuthentication()
                .SetBasicAuthentication("user", "pass")
                .ClearAuthentication()
                .SetApiKeyAuthentication("key", "X-Key");

            // Assert
            Assert.IsAssignableFrom<IHttpApiClient>(result);
        }
    }
}
