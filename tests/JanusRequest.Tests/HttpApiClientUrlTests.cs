using JanusRequest.Builders;

namespace JanusRequest.Tests
{
    public class HttpApiClientUrlTests : HttpApiClientTestBase
    {
        [Fact]
        public void JoinUrl_WithMultipleParts_JoinsCorrectly()
        {
            // Act
            //var result = new UrlQueryBuilder().BuildUrl("https://api.com/", "/users/", "123");

            //// Assert
            //Assert.Equal("https://api.com/users/123", result);
        }

        [Fact]
        public void JoinUrl_WithEmptyParts_IgnoresEmptyParts()
        {
            // Arrange
            var result = new UrlQueryBuilder().BuildUrl("https://api.com", "", "users");

            // Assert
            Assert.Equal("https://api.com/users", result);
        }

        [Fact]
        public void JoinUrl_WithNullParts_IgnoresNullParts()
        {
            // Act
            var result = new UrlQueryBuilder().BuildUrl("https://api.com", null, "users");

            // Assert
            Assert.Equal("https://api.com/users", result);
        }

        [Fact]
        public void CreateHttpRequestMessage_WithNullUrlAndPath_ThrowsInvalidOperationException()
        {
            // Arrange
            var client = new HttpApiClient(null!);
            var info = new HttpRequestInfo();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => client.CreateHttpRequestMessage(info, null));
            Assert.Contains("A URL must be defined.", ex.Message);

            client.Dispose();
        }
    }
}
