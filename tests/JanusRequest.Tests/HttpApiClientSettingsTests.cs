using JanusRequest.Builders;
using JanusRequest.ContentTranslator;
using JanusRequest.HttpHandlers;
using NSubstitute;

namespace JanusRequest.Tests
{
    public class HttpApiClientSettingsTests : HttpApiClientTestBase
    {
        [Fact]
        public void Default_SetNull_ThrowsArgumentNullException()
        {
            // Arrange
            var original = HttpApiClientSettings.Default;

            try
            {
                // Act & Assert
                Assert.Throws<ArgumentNullException>(() => HttpApiClientSettings.Default = null!);
            }
            finally
            {
                HttpApiClientSettings.Default = original;
            }
        }

        [Fact]
        public void Default_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var original = HttpApiClientSettings.Default;
            var newSettings = new HttpApiClientSettings();

            try
            {
                // Act
                HttpApiClientSettings.Default = newSettings;

                // Assert
                Assert.Same(newSettings, HttpApiClientSettings.Default);
            }
            finally
            {
                HttpApiClientSettings.Default = original;
            }
        }

        [Fact]
        public void Default_ConcurrentAccess_DoesNotThrow()
        {
            // Arrange
            var original = HttpApiClientSettings.Default;
            var exceptions = new List<Exception>();

            try
            {
                // Act - concurrent reads and writes
                var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
                {
                    try
                    {
                        if (i % 2 == 0)
                        {
                            var settings = new HttpApiClientSettings();
                            HttpApiClientSettings.Default = settings;
                        }
                        else
                        {
                            _ = HttpApiClientSettings.Default;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) exceptions.Add(ex);
                    }
                })).ToArray();

                Task.WaitAll(tasks);

                // Assert
                Assert.Empty(exceptions);
            }
            finally
            {
                HttpApiClientSettings.Default = original;
            }
        }

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
        public void DefaultMediaType_CanBeSet()
        {
            // Act
            _settings.DefaultMediaType = HttpContentType.Xml;

            // Assert
            Assert.Equal(HttpContentType.Xml, _settings.DefaultMediaType);
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
