namespace JanusRequest.Tests
{
    public class HttpApiClientAuthenticationTests : HttpApiClientTestBase
    {
        [Fact]
        public void SetBasicAuthentication_SetsAuthenticator()
        {
            // Act
            _httpApiClient.SetBasicAuthentication("user", "password");

            // Assert
            var auth = Assert.IsType<AuthorizationHeaderAuthenticator>(_httpApiClient.Settings.Authenticator);
            Assert.Equal("Basic", auth.Scheme);
            Assert.Equal(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:password")), auth.Value);
        }

        [Fact]
        public void SetBearerAuthentication_SetsAuthenticator()
        {
            // Act
            _httpApiClient.SetBearerAuthentication("token123");

            // Assert
            var auth = Assert.IsType<AuthorizationHeaderAuthenticator>(_httpApiClient.Settings.Authenticator);
            Assert.Equal("Bearer", auth.Scheme);
            Assert.Equal("token123", auth.Value);
        }

        [Fact]
        public void SetApiKeyAuthentication_SetsCustomHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123", "X-Custom-Key");

            // Assert
            var auth = Assert.IsType<ApiKeyAuthenticator>(_httpApiClient.Settings.Authenticator);
            Assert.Equal("key123", auth.ApiKey);
            Assert.Equal("X-Custom-Key", auth.HeaderName);
        }

        [Fact]
        public void SetApiKeyAuthentication_WithDefaultHeader_SetsXApiKeyHeader()
        {
            // Act
            _httpApiClient.SetApiKeyAuthentication("key123");

            // Assert
            var auth = Assert.IsType<ApiKeyAuthenticator>(_httpApiClient.Settings.Authenticator);
            Assert.Equal("key123", auth.ApiKey);
            Assert.Equal("X-API-Key", auth.HeaderName);
        }

        [Fact]
        public void ClearAuthentication_RemovesAuthenticator()
        {
            // Arrange
            _httpApiClient.SetBearerAuthentication("token");

            // Act
            _httpApiClient.ClearAuthentication();

            // Assert
            Assert.Null(_httpApiClient.Settings.Authenticator);
        }

        [Fact]
        public void SetAuthentication_WithCustomScheme_SetsAuthenticator()
        {
            // Act
            _httpApiClient.SetAuthentication("Custom", "custom-token");

            // Assert
            var auth = Assert.IsType<AuthorizationHeaderAuthenticator>(_httpApiClient.Settings.Authenticator);
            Assert.Equal("Custom", auth.Scheme);
            Assert.Equal("custom-token", auth.Value);
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
