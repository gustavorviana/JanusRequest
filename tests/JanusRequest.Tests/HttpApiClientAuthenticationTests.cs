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
    }
}
