using JanusRequest.Attributes;
using JanusRequest.Builders;

namespace JanusRequest.Tests
{
    public class UrlQueryBuilderTests
    {
        [Fact]
        public void Constructor_WithoutFormatProvider_ShouldCreateInstance()
        {
            // Arrange & Act
            var urlQuery = new UrlQueryBuilder();

            // Assert
            Assert.NotNull(urlQuery);
        }

        [Fact]
        public void Constructor_WithFormatProvider_ShouldCreateInstance()
        {
            // Arrange
            var settings = new HttpApiClientSettings();

            // Act
            var urlQuery = new UrlQueryBuilder(settings);

            // Assert
            Assert.NotNull(urlQuery);
        }

        [Fact]
        public void Set_WithStringValue_ShouldAddToQuery()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("name", "john");
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?name=john", result);
        }

        [Fact]
        public void Set_WithIntValue_ShouldAddToQuery()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("id", 123);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?id=123", result);
        }

        [Fact]
        public void Set_WithNullValue_ShouldAddEmptyValue()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("value", null);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?value=", result);
        }

        [Fact]
        public void Set_WithDateTimeValue_ShouldFormatCorrectly()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var dateTime = new DateTime(2023, 12, 25, 15, 30, 45);

            // Act
            urlQuery.Set("date", dateTime);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?date=2023-12-25+15%3a30%3a45", result);
        }

        [Fact]
        public void Set_WithMultipleValues_ShouldJoinWithAmpersand()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("name", "john")
                    .Set("age", 25)
                    .Set("active", true);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?name=john&age=25&active=True", result);
        }

        [Fact]
        public void Set_WithSpecialCharacters_ShouldUrlEncode()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("search", "hello world & test");
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?search=hello+world+%26+test", result);
        }

        [Fact]
        public void ToString_WithEmptyQuery_ShouldReturnEmptyString()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            var result = urlQuery.ToString();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void AddRange_WithKeyValuePairs_ShouldAddAllItems()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var items = new List<KeyValuePair<string, string>>
            {
                new("key1", "value1"),
                new("key2", "value2")
            };

            // Act
            urlQuery.AddRange(items);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?key1=value1&key2=value2", result);
        }

        [Fact]
        public void Merge_WithNullQuery_ShouldReturnOriginalQuery()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            urlQuery.Set("original", "value");

            // Act
            var result = urlQuery.Merge(null);

            // Assert
            Assert.Equal("?original=value", result.ToString());
        }

        [Fact]
        public void Merge_WithAnotherQuery_ShouldCombineBoth()
        {
            // Arrange
            var query1 = new UrlQueryBuilder();
            query1.Set("key1", "value1");

            var query2 = new UrlQueryBuilder();
            query2.Set("key2", "value2");

            // Act
            var result = query1.Merge(query2);

            // Assert
            var resultString = result.ToString();
            Assert.Equal("?key1=value1&key2=value2", resultString);
        }

        [Fact]
        public void AddAll_WithAnotherQuery_ShouldAddAllItems()
        {
            // Arrange
            var query1 = new UrlQueryBuilder();
            query1.Set("existing", "value");

            var query2 = new UrlQueryBuilder();
            query2.Set("new", "item");

            // Act
            query1.AddAll(query2);
            var result = query1.ToString();

            // Assert
            Assert.Equal("?existing=value&new=item", result);
        }

        [Fact]
        public void Add_WithSimpleObject_ShouldAddProperties()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new { Name = "John", Age = 30 };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?Name=John&Age=30", result);
        }

        [Fact]
        public void Add_WithNullObject_ShouldNotThrow()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            var exception = Record.Exception(() => urlQuery.Add(null));

            // Assert
            Assert.Null(exception);
            Assert.Equal("", urlQuery.ToString());
        }

        [Fact]
        public void Add_WithObjectHavingNullProperties_ShouldIgnoreNullProperties()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new { Name = "John", Age = (int?)null };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?Name=John", result);
        }

        [Fact]
        public void Add_WithQueryIgnoreAttribute_ShouldIgnoreProperty()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectWithIgnore { Name = "John", Password = "secret" };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?Name=John", result);
        }

        [Fact]
        public void Add_WithQueryArgAttribute_ShouldUseCustomName()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectWithCustomName { UserId = 123 };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?user_id=123", result);
        }

        [Fact]
        public void Add_WithAttributesOnly_ShouldOnlyIncludePropertiesWithAttributes()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectMixed
            {
                Name = "John",
                UserId = 123,
                Email = "john@example.com"
            };

            // Act
            urlQuery.Add(obj, strictQueryArgs: true);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?user_id=123", result);
        }

        [Fact]
        public void Add_WithNestedObject_ShouldCreateNestedKeys()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new
            {
                User = new
                {
                    Name = "John",
                    Age = 30
                }
            };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?User.Name=John&User.Age=30", result);
        }

        [Fact]
        public void Add_WithCollection_ShouldCreateIndexedKeys()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new
            {
                Tags = new[] { "tag1", "tag2", "tag3" }
            };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?Tags=tag1%2ctag2%2ctag3", result);
        }

        [Fact]
        public void Set_SameKeyTwice_ShouldOverwriteValue()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("key", "value1")
                    .Set("key", "value2");
            var result = urlQuery.ToString();

            // Assert
            Assert.Equal("?key=value2", result);
        }

        [Fact]
        public void Add_WithStrictQueryArgs_OnlyIncludesQueryArgProperties()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectMixed
            {
                Name = "John",
                UserId = 123,
                Email = "john@example.com"
            };

            // Act
            urlQuery.Add(obj, strictQueryArgs: true);
            var result = urlQuery.ToString();

            // Assert
            Assert.Contains("user_id=123", result);
            Assert.DoesNotContain("Name", result);
            Assert.DoesNotContain("Email", result);
        }

        [Fact]
        public void Add_WithStrictQueryArgsFalse_IncludesAllProperties()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectMixed
            {
                Name = "John",
                UserId = 123,
                Email = "john@example.com"
            };

            // Act
            urlQuery.Add(obj, strictQueryArgs: false);
            var result = urlQuery.ToString();

            // Assert
            Assert.Contains("Name=John", result);
            Assert.Contains("user_id=123", result);
            Assert.Contains("Email=john%40example.com", result);
        }

        [Fact]
        public void Add_WithDeeplyNestedObject_ProducesDottedPaths()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new DeepNested
            {
                Inner = new DeepNested.InnerLevel
                {
                    Value = "deep"
                }
            };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert
            Assert.Contains("Inner.Value=deep", result);
        }

        [Fact]
        public void BuildUrl_WithSinglePart_ReturnsUrlWithQuery()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            urlQuery.Set("key", "value");

            // Act
            var result = urlQuery.BuildUrl("https://api.example.com");

            // Assert
            Assert.Equal("https://api.example.com?key=value", result);
        }

        [Fact]
        public void BuildUrl_WithMultipleParts_JoinsWithSlash()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            urlQuery.Set("id", 1);

            // Act
            var result = urlQuery.BuildUrl("https://api.example.com", "/users");

            // Assert
            Assert.Equal("https://api.example.com/users?id=1", result);
        }

        [Fact]
        public void BuildUrl_WithTrailingAndLeadingSlashes_TrimsCorrectly()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            var result = urlQuery.BuildUrl("https://api.example.com/", "/users/", "/42");

            // Assert
            Assert.Equal("https://api.example.com/users/42", result);
        }

        [Fact]
        public void BuildUrl_WithEmptyQuery_ReturnsUrlOnly()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            var result = urlQuery.BuildUrl("https://api.example.com", "/users");

            // Assert
            Assert.Equal("https://api.example.com/users", result);
        }

        [Fact]
        public void AddAll_WithNull_ReturnsThis()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            urlQuery.Set("key", "value");

            // Act
            var result = urlQuery.AddAll(null);

            // Assert
            Assert.Same(urlQuery, result);
            Assert.Equal("?key=value", urlQuery.ToString());
        }

        [Fact]
        public void Build_EncodesQueryKeys()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();

            // Act
            urlQuery.Set("my key", "value");
            var result = urlQuery.ToString();

            // Assert
            Assert.Contains("my+key=value", result);
        }

        [Fact]
        public void GetValue_CollectionWithNullItems_NoDoubleComma()
        {
            // Arrange
            var urlQuery = new UrlQueryBuilder();
            var obj = new TestObjectWithNullableList
            {
                Tags = new List<string> { "a", null, "b" }
            };

            // Act
            urlQuery.Add(obj);
            var result = urlQuery.ToString();

            // Assert - commas are URL-encoded as %2c; double comma would be %2c%2c
            Assert.DoesNotContain("%2c%2c", result);
            Assert.Contains("a%2cb", result);
        }

        // Helper classes for testing
        private class DeepNested
        {
            public InnerLevel Inner { get; set; }

            public class InnerLevel
            {
                public string Value { get; set; }
            }
        }

        private class TestObjectWithIgnore
        {
            public string? Name { get; set; }

            [QueryIgnore]
            public string? Password { get; set; }
        }

        private class TestObjectWithCustomName
        {
            [QueryArg("user_id")]
            public int UserId { get; set; }
        }

        private class TestObjectMixed
        {
            public string? Name { get; set; }

            [QueryArg("user_id")]
            public int UserId { get; set; }

            public string? Email { get; set; }
        }

        private class TestObjectWithNullableList
        {
            public List<string?> Tags { get; set; } = new List<string?>();
        }
    }
}