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

        public class TestRequestWithAttributes
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            [QueryArg("q")]
            public string? Search { get; set; }

            [PathOnly]
            public int ResourceId { get; set; }
        }
    }
}
