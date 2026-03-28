using JanusRequest.Attributes;

namespace JanusRequest.Json.Newtonsoft.Tests
{
    public class NewtonsoftJsonContentTranslatorTests
    {
        private readonly NewtonsoftJsonContentTranslator _translator = new();

        [Fact]
        public void Serialize_ExcludesQueryArgProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                Search = "should-be-excluded",
                ResourceId = 99
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("Search", json);
            Assert.DoesNotContain("should-be-excluded", json);
        }

        [Fact]
        public void Serialize_ExcludesPathOnlyProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                Search = "query",
                ResourceId = 99
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("ResourceId", json);
            Assert.DoesNotContain("99", json);
        }

        [Fact]
        public void Serialize_IncludesRegularProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 42,
                Name = "Regular",
                Search = "excluded",
                ResourceId = 99
            };

            var json = _translator.Serialize(request);

            Assert.Contains("\"Id\":42", json);
            Assert.Contains("\"Name\":\"Regular\"", json);
        }

        [Fact]
        public void Deserialize_SingleObject_DeserializesCorrectly()
        {
            var json = "{\"Id\":1,\"Name\":\"Test\"}";

            var result = _translator.Deserialize<TestResponse>(json);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public void Deserialize_Array_DeserializesCorrectly()
        {
            var json = "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]";

            var result = _translator.Deserialize<TestResponse[]>(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("A", result[0].Name);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("B", result[1].Name);
        }

        [Fact]
        public void Deserialize_IList_DeserializesCorrectly()
        {
            var json = "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]";

            var result = _translator.Deserialize<IList<TestResponse>>(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("A", result[0].Name);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("B", result[1].Name);
        }

        [Fact]
        public void Deserialize_ICollection_DeserializesCorrectly()
        {
            var json = "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]";

            var result = _translator.Deserialize<ICollection<TestResponse>>(json);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            var items = result.ToArray();
            Assert.Equal(1, items[0].Id);
            Assert.Equal("A", items[0].Name);
            Assert.Equal(2, items[1].Id);
            Assert.Equal("B", items[1].Name);
        }

        [Fact]
        public void Deserialize_Null_ReturnsDefault()
        {
            var result = _translator.Deserialize<TestResponse>(null!);

            Assert.Null(result);
        }

        [Fact]
        public async Task Parse_WithValidObject_ReturnsStringContent()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                Search = "excluded",
                ResourceId = 99
            };

            var content = _translator.Parse(request);

            Assert.NotNull(content);
            var json = await content.ReadAsStringAsync();
            Assert.Contains("\"Id\":1", json);
            Assert.Contains("\"Name\":\"Test\"", json);
            Assert.DoesNotContain("Search", json);
            Assert.DoesNotContain("ResourceId", json);
        }

        [Fact]
        public void Serialize_ExcludesHeaderProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                HeaderParam = "should-be-excluded"
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("HeaderParam", json);
            Assert.DoesNotContain("should-be-excluded", json);
            Assert.Contains("\"Id\":1", json);
        }

        [Fact]
        public void Serialize_ExcludesHeaderCollectionProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                HeaderCollectionParam = new Dictionary<string, string> { ["X-Test"] = "value" }
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("HeaderCollectionParam", json);
            Assert.Contains("\"Id\":1", json);
        }

        [Fact]
        public void Serialize_ExcludesCookieProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                CookieParam = "should-be-excluded"
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("CookieParam", json);
            Assert.DoesNotContain("should-be-excluded", json);
            Assert.Contains("\"Id\":1", json);
        }

        [Fact]
        public void Serialize_ExcludesCookieCollectionProperties()
        {
            var request = new TestRequestWithAttributes
            {
                Id = 1,
                Name = "Test",
                CookieCollectionParam = new Dictionary<string, string> { ["session"] = "value" }
            };

            var json = _translator.Serialize(request);

            Assert.DoesNotContain("CookieCollectionParam", json);
            Assert.Contains("\"Id\":1", json);
        }

        [Fact]
        public void Parse_WithNull_ReturnsNull()
        {
            var content = _translator.Parse(null!);

            Assert.Null(content);
        }

        public class TestResponse
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        [Fact]
        public void Serialize_ByteArray_SerializesAsBase64StringByDefault()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var request = new TestRequestWithBase64 { ImageData = bytes };

            var json = _translator.Serialize(request);

            var expected = Convert.ToBase64String(bytes);
            Assert.Contains($"\"{expected}\"", json);
            Assert.DoesNotContain("[1,2,3,4,5]", json);
        }

        [Fact]
        public void Serialize_Stream_SerializesAsBase64StringByDefault()
        {
            var bytes = new byte[] { 10, 20, 30, 40, 50 };
            var request = new TestRequestWithBase64 { StreamData = new MemoryStream(bytes) };

            var json = _translator.Serialize(request);

            var expected = Convert.ToBase64String(bytes);
            Assert.Contains($"\"{expected}\"", json);
        }

        [Fact]
        public void Serialize_ByteArrayWithRawBytesAttribute_UsesDefaultSerializerBehavior()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var request = new TestRequestWithBase64 { RawData = bytes };

            var json = _translator.Serialize(request);

            // Newtonsoft.Json default for byte[] is also base64, so [RawBytes] preserves that default.
            var expected = Convert.ToBase64String(bytes);
            Assert.Contains($"\"{expected}\"", json);
        }

        [Fact]
        public void Serialize_WhenBase64PropertyIsNull_SerializesAsNull()
        {
            var request = new TestRequestWithBase64 { ImageData = null };

            var json = _translator.Serialize(request);

            Assert.Contains("null", json);
        }

        [Fact]
        public void Deserialize_Base64String_DeserializesToByteArray()
        {
            var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
            var base64 = Convert.ToBase64String(originalBytes);
            var json = $"{{\"ImageData\":\"{base64}\"}}";

            var result = _translator.Deserialize<TestResponseWithBase64>(json);

            Assert.NotNull(result);
            Assert.Equal(originalBytes, result.ImageData);
        }

        [Fact]
        public void Deserialize_Base64String_DeserializesToStream()
        {
            var originalBytes = new byte[] { 10, 20, 30 };
            var base64 = Convert.ToBase64String(originalBytes);
            var json = $"{{\"StreamData\":\"{base64}\"}}";

            var result = _translator.Deserialize<TestResponseWithBase64>(json);

            Assert.NotNull(result);
            Assert.NotNull(result.StreamData);
            Assert.IsType<MemoryStream>(result.StreamData);
            using var ms = (MemoryStream)result.StreamData;
            Assert.Equal(originalBytes, ms.ToArray());
        }

        [Fact]
        public void Serialize_Deserialize_ByteArray_RoundTrip()
        {
            var originalBytes = new byte[] { 0, 127, 255, 1, 100 };
            var request = new TestRequestWithBase64 { Name = "test", ImageData = originalBytes };

            var json = _translator.Serialize(request);
            var result = _translator.Deserialize<TestResponseWithBase64>(json);

            Assert.Equal("test", result.Name);
            Assert.Equal(originalBytes, result.ImageData);
        }

        public class TestResponseWithBase64
        {
            public string? Name { get; set; }
            public byte[]? ImageData { get; set; }
            public Stream? StreamData { get; set; }
        }

        public class TestRequestWithBase64
        {
            public string? Name { get; set; }
            public byte[]? ImageData { get; set; }
            public Stream? StreamData { get; set; }

            [RawBytes]
            public byte[]? RawData { get; set; }
        }

        public class TestRequestWithAttributes
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            [QueryArg("q")]
            public string? Search { get; set; }

            [PathOnly]
            public int ResourceId { get; set; }

            [Header("X-Custom")]
            public string? HeaderParam { get; set; }

            [HeaderCollection]
            public Dictionary<string, string>? HeaderCollectionParam { get; set; }

            [Cookie("session")]
            public string? CookieParam { get; set; }

            [CookieCollection]
            public Dictionary<string, string>? CookieCollectionParam { get; set; }
        }
    }
}
