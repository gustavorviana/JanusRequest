using JanusRequest.ContentTranslator;

namespace JanusRequest.Tests
{
    public class HttpApiClientSettingsMediaTypeTests
    {
        [Fact]
        public void Deserialize_WithStructuredSuffix_FallsBackToBaseJsonTranslator_WhenSpecificNotRegistered()
        {
            // Arrange: only default JsonContentTranslator ("application/json") registered
            var settings = new HttpApiClientSettings();

            const string contentType = "application/error+json";
            var json = "{\"Name\":\"Test\"}";

            // Act
            var result = settings.Deserialize<TestDto>(json, contentType);

            // Assert: should still deserialize successfully via base JSON translator
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public void Deserialize_WithSpecificStructuredMediaType_UsesSpecificTranslatorOverBase()
        {
            // Arrange
            var settings = new HttpApiClientSettings();

            var specificTranslator = new TestJsonContentTranslator("application/error+json");
            settings.SetContentBuilder(specificTranslator);

            const string contentType = "application/error+json";
            var json = "{\"Name\":\"Test\"}";

            // Act
            var result = settings.Deserialize<TestDto>(json, contentType);

            // Assert: our specific translator was used instead of the base one
            Assert.NotNull(result);
            Assert.True(specificTranslator.WasDeserializeCalled);
        }

        private sealed class TestDto
        {
            public string Name { get; set; } = string.Empty;
        }

        private sealed class TestJsonContentTranslator : ContentTypeTranslator
        {
            private readonly string _contentType;

            public bool WasDeserializeCalled { get; private set; }

            public TestJsonContentTranslator(string contentType)
            {
                _contentType = contentType;
            }

            public override string ContentType => _contentType;

            public override HttpContent Parse(object content)
            {
                // Not relevant for this test
                return null!;
            }

            public override string Serialize(object content)
            {
                // Not relevant for this test
                return string.Empty;
            }

            public override TResponse Deserialize<TResponse>(string response)
            {
                WasDeserializeCalled = true;
                return (TResponse)(object)new TestDto { Name = "Test" }
                    ?? throw new InvalidOperationException("Unexpected target type.");
            }
        }
    }
}

