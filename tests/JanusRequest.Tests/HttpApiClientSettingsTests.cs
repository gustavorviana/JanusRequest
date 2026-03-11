using JanusRequest.Builders;
using JanusRequest.ContentTranslator;
using JanusRequest.HttpHandlers;
using NSubstitute;

namespace JanusRequest.Tests
{
    public class HttpApiClientSettingsTests : HttpApiClientTestBase
    {
        [Fact]
        public void SetHandlers_SetsHandlersArray()
        {
            // Arrange
            var handler1 = Substitute.For<IHttpHandlerBase>();
            var handler2 = Substitute.For<IHttpHandlerBase>();

            // Act
            var result = _settings.SetHandlers(handler1, handler2);

            // Assert
            Assert.Same(_settings, result); // Fluent interface
        }

        [Fact]
        public void SetHandlers_WithNullHandlers_AcceptsNull()
        {
            // Act
            var result = _settings.SetHandlers(null);

            // Assert
            Assert.Same(_settings, result);
        }

        [Fact]
        public void SetContentBuilder_AddsContentTranslators()
        {
            // Arrange
            var jsonTranslator = Substitute.For<ContentTypeTranslator>();
            jsonTranslator.ContentType.Returns(HttpContentType.Json);

            // Act
            var result = _settings.SetContentBuilder(jsonTranslator);

            // Assert
            Assert.Same(_settings, result); // Fluent interface
        }

        [Fact]
        public void DefaultContentType_CanBeSet()
        {
            // Act
            _settings.DefaultContentType = HttpContentType.Xml;

            // Assert
            Assert.Equal(HttpContentType.Xml, _settings.DefaultContentType);
        }

        [Fact]
        public void DefaultArgs_CanBeSet()
        {
            // Arrange
            var queryBuilder = new UrlQueryBuilder();
            queryBuilder.Set("param", "value");

            // Act
            _httpApiClient.DefaultArgs = queryBuilder;

            // Assert
            Assert.Same(queryBuilder, _httpApiClient.DefaultArgs);
        }

        [Fact]
        public void DefaultHeaders_ReturnsHttpClientHeaders()
        {
            // Act
            var headers = _httpApiClient.DefaultHeaders;

            // Assert
            Assert.Same(_httpClient.DefaultRequestHeaders, headers);
        }
    }
}
