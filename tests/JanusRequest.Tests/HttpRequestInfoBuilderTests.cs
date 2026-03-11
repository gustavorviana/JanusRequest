using JanusRequest.Attributes;
using JanusRequest.Builders;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpRequestInfoBuilderTests
    {
        [Fact]
        public void Constructor_Default_ShouldCreateInstance()
        {
            // Act
            var builder = new HttpRequestInfoBuilder();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void Constructor_WithHttpRequestInfo_ShouldCopyAllProperties()
        {
            // Arrange
            var originalQuery = new UrlQueryBuilder().Set("param", "value");
            var originalHeaders = new WebHeaderCollection { ["Authorization"] = "Bearer token" };
            var originalCookies = new CookieCollection
            {
                new Cookie("sessionId", "123")
            };

            var info = new HttpRequestInfo
            {
                Path = "/api/users",
                Method = "POST",
                Query = originalQuery,
                Headers = originalHeaders,
                Cookies = originalCookies
            };

            // Act
            var builder = new HttpRequestInfoBuilder(info);
            var result = builder.Build();

            // Assert
            Assert.Equal("/api/users", result.Path);
            Assert.Equal("POST", result.Method);
            Assert.Equal("Bearer token", result.Headers["Authorization"]);
            Assert.Single(result.Cookies);
        }

        [Fact]
        public void SetPath_WithNullOrEmpty_ShouldIgnore()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder().SetPath("/original");

            // Act
            var result1 = builder.SetPath(null).Build();
            var result2 = builder.SetPath("").Build();
            var result3 = builder.SetPath("   ").Build();

            // Assert
            Assert.Equal("/original", result1.Path);
            Assert.Equal("/original", result2.Path);
            Assert.Equal("/original", result3.Path);
        }

        [Fact]
        public void SetPath_WithWhitespace_ShouldTrim()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();

            // Act
            var result = builder.SetPath("  /api/users  ").Build();

            // Assert
            Assert.Equal("/api/users", result.Path);
        }

        [Fact]
        public void SetMethod_WithValidMethod_ShouldSetMethod()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();

            // Act
            var result = builder.SetMethod("POST").Build();

            // Assert
            Assert.Equal("POST", result.Method);
        }

        [Fact]
        public void SetMethod_WithNullOrEmpty_ShouldIgnore()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder().SetMethod("POST");

            // Act
            var result1 = builder.SetMethod(null).Build();
            var result2 = builder.SetMethod("").Build();

            // Assert
            Assert.Equal("POST", result1.Method);
            Assert.Equal("POST", result2.Method);
        }

        [Fact]
        public void AddQuery_WithUrlQueryBuilder_ShouldMergeQueries()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var query1 = new UrlQueryBuilder().Set("param1", "value1");
            var query2 = new UrlQueryBuilder().Set("param2", "value2");

            // Act
            var result = builder.AddQuery(query1).AddQuery(query2).Build();

            // Assert
            var queryString = result.Query.ToString();
            Assert.Contains("param1=value1", queryString);
            Assert.Contains("param2=value2", queryString);
        }

        [Fact]
        public void AddHeader_WithWebHeaderCollection_ShouldAddHeaders()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var headers = new WebHeaderCollection
            {
                ["Authorization"] = "Bearer token",
                ["Content-Type"] = "application/json"
            };

            // Act
            var result = builder.AddHeader(headers).Build();

            // Assert
            Assert.Equal("Bearer token", result.Headers["Authorization"]);
            Assert.Equal("application/json", result.Headers["Content-Type"]);
        }

        [Fact]
        public void AddCookie_WithSingleCookie_ShouldAddCookie()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var cookie = new Cookie("sessionId", "abc123");

            // Act
            var result = builder.AddCookie(cookie).Build();

            // Assert
            Assert.Single(result.Cookies);
            Assert.Equal("abc123", result.Cookies["sessionId"]!.Value);
        }

        [Fact]
        public void AddCookie_WithCookieCollection_ShouldAddAllCookies()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var cookies = new CookieCollection
            {
                new Cookie("cookie1", "value1"),
                new Cookie("cookie2", "value2")
            };

            // Act
            var result = builder.AddCookie(cookies).Build();

            // Assert
            Assert.Equal(2, result.Cookies.Count);
            Assert.Equal("value1", result.Cookies["cookie1"]!.Value);
            Assert.Equal("value2", result.Cookies["cookie2"]!.Value);
        }

        [Fact]
        public void AddCookie_WithDuplicateName_ShouldOverwrite()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var cookie1 = new Cookie("sessionId", "old_value");
            var cookie2 = new Cookie("sessionId", "new_value");

            // Act
            var result = builder.AddCookie(cookie1).AddCookie(cookie2).Build();

            // Assert
            Assert.Single(result.Cookies);
            Assert.Equal("new_value", result.Cookies["sessionId"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithNull_ShouldReturnThis()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();

            // Act
            var result = builder.ApplyRequestObject(null);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void ApplyRequestObject_WithNativeType_ShouldReturnThis()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();

            // Act
            var result = builder.ApplyRequestObject("string value");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void ApplyRequestObject_WithRequestAttribute_ShouldApplyMethodAndPath()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithAttribute();

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("POST", result.Method);
            Assert.Equal("/api/test", result.Path);
        }

        [Fact]
        public void ApplyRequestObject_WithRequestAttributeButExistingValues_ShouldNotOverwrite()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder()
                .SetMethod("PUT")
                .SetPath("/existing/path");
            var request = new TestRequestWithAttribute();

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("PUT", result.Method);
            Assert.Equal("/existing/path", result.Path);
        }

        [Fact]
        public void Build_WithDefaultValues_ShouldReturnDefaultHttpRequestInfo()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("GET", result.Method);
            Assert.NotNull(result.Headers);
            Assert.NotNull(result.Cookies);
            Assert.NotNull(result.Query);
        }

        [Fact]
        public void Build_WithPathTemplate_ShouldBuildPathWithRequestObject()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder()
                .SetPath("/api/users/{id}")
                .ApplyRequestObject(new { id = 123 });

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("/api/users/123", result.Path);
        }

        [Fact]
        public void Build_WithGETMethod_ShouldAddRequestObjectToQuery()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder()
                .SetMethod("GET")
                .ApplyRequestObject(new { name = "john", age = 30 });

            // Act
            var result = builder.Build();

            // Assert
            var queryString = result.Query.ToString();
            Assert.Contains("name=john", queryString);
            Assert.Contains("age=30", queryString);
        }

        [Fact]
        public void Build_WithNonGETMethod_ShouldNotAddRequestObjectToQuery()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder()
                .SetMethod("POST")
                .ApplyRequestObject(new { name = "john", age = 30 });

            // Act
            var result = builder.Build();

            // Assert
            Assert.Empty(result.Query.ToString());
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        public void Build_WithNonGETMethods_ShouldNotAddRequestObjectToQuery(string method)
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder()
                .SetMethod(method)
                .ApplyRequestObject(new { data = "test" });

            // Act
            var result = builder.Build();

            // Assert
            Assert.Empty(result.Query.ToString());
        }

        #region Header Attribute Tests

        [Fact]
        public void ApplyRequestObject_WithHeaderAttribute_AddsHeaderToResult()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeader { Token = "my-token" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("my-token", result.Headers["X-Custom-Header"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderAttribute_IntValue_UsesToString()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithIntHeader { RequestId = 42 };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("42", result.Headers["X-Request-Id"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderAttribute_NullValue_DoesNotAddHeader()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeader { Token = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Null(result.Headers["X-Custom-Header"]);
        }

        [Fact]
        public void ApplyRequestObject_WithMultipleHeaderAttributes_AddsAllHeaders()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleHeaders
            {
                Authorization = "Bearer token123",
                ApiKey = "key-abc"
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("Bearer token123", result.Headers["Authorization"]);
            Assert.Equal("key-abc", result.Headers["X-Api-Key"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderAndQueryArg_BothWorkIndependently()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeaderAndQueryArg
            {
                Token = "my-token",
                Search = "test-query",
                Body = "body-value"
            };

            // Act
            var result = builder.SetMethod("GET").ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("my-token", result.Headers["X-Token"]);
            Assert.Contains("search=test-query", result.Query.ToString());
        }

        #endregion

        #region HeaderCollection Attribute Tests

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_Dictionary_AddsAllEntries()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeaderCollection
            {
                CustomHeaders = new Dictionary<string, string>
                {
                    ["X-First"] = "value1",
                    ["X-Second"] = "value2"
                }
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("value1", result.Headers["X-First"]);
            Assert.Equal("value2", result.Headers["X-Second"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_NullValue_DoesNotThrow()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeaderCollection { CustomHeaders = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_EmptyDictionary_DoesNotAddHeaders()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeaderCollection
            {
                CustomHeaders = new Dictionary<string, string>()
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(0, result.Headers.Count);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_ReadOnlyDictionary_Works()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var dict = new Dictionary<string, string>
            {
                ["X-Read-Only"] = "ro-value"
            };
            var request = new TestRequestWithReadOnlyDictionaryHeaders
            {
                Headers = dict
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("ro-value", result.Headers["X-Read-Only"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_Convertible_UsesInterface()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithConvertibleHeaders
            {
                Headers = new CustomHeaderSource("X-From-Convertible", "converted-value")
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("converted-value", result.Headers["X-From-Convertible"]);
        }

        [Fact]
        public void ApplyRequestObject_WithHeaderCollectionAttribute_EmptyKeyInDictionary_SkipsEntry()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithHeaderCollectionObjectDict
            {
                CustomHeaders = new Dictionary<string, object>
                {
                    ["X-Valid"] = "value",
                    [""] = "should-be-skipped"
                }
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("value", result.Headers["X-Valid"]);
            Assert.Equal(1, result.Headers.Count);
        }

        #endregion

        #region Cookie Attribute Tests

        [Fact]
        public void ApplyRequestObject_WithCookieAttribute_AddsCookieToResult()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookie { SessionId = "abc123" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(1, result.Cookies.Count);
            Assert.Equal("abc123", result.Cookies["session"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieAttribute_NullValue_DoesNotAddCookie()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookie { SessionId = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(0, result.Cookies.Count);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieAttribute_IntValue_UsesToString()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithIntCookie { MaxAge = 3600 };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("3600", result.Cookies["max-age"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithMultipleCookieAttributes_AddsAllCookies()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleCookies
            {
                SessionId = "sess123",
                Preferences = "dark-mode"
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(2, result.Cookies.Count);
            Assert.Equal("sess123", result.Cookies["session"]!.Value);
            Assert.Equal("dark-mode", result.Cookies["preferences"]!.Value);
        }

        #endregion

        #region CookieCollection Attribute Tests

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionAttribute_Dictionary_AddsAllEntries()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookieCollection
            {
                Cookies = new Dictionary<string, string>
                {
                    ["session"] = "abc123",
                    ["theme"] = "dark"
                }
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(2, result.Cookies.Count);
            Assert.Equal("abc123", result.Cookies["session"]!.Value);
            Assert.Equal("dark", result.Cookies["theme"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionAttribute_NullValue_DoesNotThrow()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookieCollection { Cookies = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(0, result.Cookies.Count);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionAttribute_ReadOnlyDictionary_Works()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var dict = new Dictionary<string, string> { ["token"] = "xyz" };
            var request = new TestRequestWithReadOnlyDictionaryCookies
            {
                Cookies = dict
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(1, result.Cookies.Count);
            Assert.Equal("xyz", result.Cookies["token"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionAttribute_Convertible_UsesInterface()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithConvertibleCookies
            {
                Cookies = new CustomCookieSource("session", "converted-value")
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(1, result.Cookies.Count);
            Assert.Equal("converted-value", result.Cookies["session"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionAttribute_EmptyKeyInDictionary_SkipsEntry()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookieCollectionObjectDict
            {
                Cookies = new Dictionary<string, object>
                {
                    ["valid"] = "value",
                    [""] = "should-be-skipped"
                }
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(1, result.Cookies.Count);
            Assert.Equal("value", result.Cookies["valid"]!.Value);
        }

        #endregion

        #region AllowMultiple Tests

        [Fact]
        public void ApplyRequestObject_WithMultipleHeaderAttributesOnSameProperty_AddsAllHeaders()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleHeadersOnSameProperty { Value = "shared-value" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("shared-value", result.Headers["X-First"]);
            Assert.Equal("shared-value", result.Headers["X-Second"]);
        }

        [Fact]
        public void ApplyRequestObject_WithMultipleHeaderAttributesOnSameProperty_NullValue_DoesNotAddHeaders()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleHeadersOnSameProperty { Value = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(0, result.Headers.Count);
        }

        [Fact]
        public void ApplyRequestObject_WithMultipleCookieAttributesOnSameProperty_AddsAllCookies()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleCookiesOnSameProperty { Value = "shared-value" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(2, result.Cookies.Count);
            Assert.Equal("shared-value", result.Cookies["cookie-a"]!.Value);
            Assert.Equal("shared-value", result.Cookies["cookie-b"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithMultipleCookieAttributesOnSameProperty_NullValue_DoesNotAddCookies()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithMultipleCookiesOnSameProperty { Value = null };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(0, result.Cookies.Count);
        }

        #endregion

        #region Cookie Path and Domain Tests

        [Fact]
        public void ApplyRequestObject_WithCookieAttributeAndPath_SetsCookiePath()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookiePath { SessionId = "abc123" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("abc123", result.Cookies["session"]!.Value);
            Assert.Equal("/api", result.Cookies["session"]!.Path);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieAttributeAndDomain_SetsCookieDomain()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookieDomain { SessionId = "abc123" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("abc123", result.Cookies["session"]!.Value);
            Assert.Equal(".example.com", result.Cookies["session"]!.Domain);
        }

        [Fact]
        public void ApplyRequestObject_WithCookieAttributeAndPathAndDomain_SetsBoth()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithCookiePathAndDomain { SessionId = "abc123" };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            var cookie = result.Cookies["session"]!;
            Assert.Equal("abc123", cookie.Value);
            Assert.Equal("/api", cookie.Path);
            Assert.Equal(".example.com", cookie.Domain);
        }

        #endregion

        #region Conflict Validation Tests

        [Fact]
        public void ApplyRequestObject_WithHeaderAndHeaderCollectionOnSameProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithConflictingHeaderAttributes
            {
                Headers = new Dictionary<string, string> { ["X-Test"] = "value" }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.ApplyRequestObject(request));
        }

        [Fact]
        public void ApplyRequestObject_WithCookieAndCookieCollectionOnSameProperty_ThrowsInvalidOperationException()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithConflictingCookieAttributes
            {
                Cookies = new Dictionary<string, string> { ["session"] = "value" }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.ApplyRequestObject(request));
        }

        #endregion

        #region ContentToString with Custom Settings Tests

        [Fact]
        public void ApplyRequestObject_WithDateTimeHeader_UsesDateTimeFormatFromSettings()
        {
            // Arrange
            var settings = new HttpApiClientSettings { DateTimeFormat = "yyyy-MM-dd" };
            var builder = new HttpRequestInfoBuilder(settings);
            var request = new TestRequestWithDateTimeHeader { Date = new DateTime(2025, 6, 15, 10, 30, 0) };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("2025-06-15", result.Headers["X-Date"]);
        }

        [Fact]
        public void ApplyRequestObject_WithDateTimeCookie_UsesDateTimeFormatFromSettings()
        {
            // Arrange
            var settings = new HttpApiClientSettings { DateTimeFormat = "dd/MM/yyyy" };
            var builder = new HttpRequestInfoBuilder(settings);
            var request = new TestRequestWithDateTimeCookie { Expires = new DateTime(2025, 12, 25) };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("25/12/2025", result.Cookies["expires"]!.Value);
        }

        [Fact]
        public void ApplyRequestObject_WithTimeSpanHeader_UsesTimeFormatFromSettings()
        {
            // Arrange
            var settings = new HttpApiClientSettings { TimeFormat = @"hh\:mm\:ss" };
            var builder = new HttpRequestInfoBuilder(settings);
            var request = new TestRequestWithTimeSpanHeader { Duration = new TimeSpan(2, 30, 45) };

            // Act — use POST to avoid query builder also formatting the TimeSpan
            var result = builder.SetMethod("POST").ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal("02:30:45", result.Headers["X-Duration"]);
        }

        [Fact]
        public void ApplyRequestObject_WithDateTimeHeader_DefaultSettings_UsesDefaultFormat()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder(); // default settings
            var date = new DateTime(2025, 6, 15, 10, 30, 0);
            var request = new TestRequestWithDateTimeHeader { Date = date };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            var expected = date.ToString(HttpApiClientSettings.Default.DateTimeFormat);
            Assert.Equal(expected, result.Headers["X-Date"]);
        }

        #endregion

        #region ICookieCollectionConvertible with Cookie objects

        [Fact]
        public void ApplyRequestObject_WithCookieCollectionConvertible_ReturningCookieObjects_PreservesPathAndDomain()
        {
            // Arrange
            var builder = new HttpRequestInfoBuilder();
            var request = new TestRequestWithConvertibleCookiesWithPathDomain
            {
                Cookies = new CustomCookieSourceWithPathDomain()
            };

            // Act
            var result = builder.ApplyRequestObject(request).Build();

            // Assert
            Assert.Equal(1, result.Cookies.Count);
            var cookie = result.Cookies["session"]!;
            Assert.Equal("abc123", cookie.Value);
            Assert.Equal("/api", cookie.Path);
            Assert.Equal(".example.com", cookie.Domain);
        }

        #endregion

        // Helper classes for testing
        [Request("/api/test", Method = "POST")]
        private class TestRequestWithAttribute
        {
            public string Name { get; set; } = "Test";
        }

        private class TestRequestWithoutAttribute
        {
            public int Id { get; set; } = 1;
        }

        private class TestRequestWithHeader
        {
            [Header("X-Custom-Header")]
            public string Token { get; set; }
        }

        private class TestRequestWithIntHeader
        {
            [Header("X-Request-Id")]
            public int RequestId { get; set; }
        }

        private class TestRequestWithMultipleHeaders
        {
            [Header("Authorization")]
            public string Authorization { get; set; }

            [Header("X-Api-Key")]
            public string ApiKey { get; set; }
        }

        private class TestRequestWithHeaderAndQueryArg
        {
            [Header("X-Token")]
            public string Token { get; set; }

            [QueryArg("search")]
            public string Search { get; set; }

            public string Body { get; set; }
        }

        private class TestRequestWithHeaderCollection
        {
            [HeaderCollection]
            public Dictionary<string, string> CustomHeaders { get; set; }
        }

        private class TestRequestWithHeaderCollectionObjectDict
        {
            [HeaderCollection]
            public Dictionary<string, object> CustomHeaders { get; set; }
        }

        private class TestRequestWithReadOnlyDictionaryHeaders
        {
            [HeaderCollection]
            public IReadOnlyDictionary<string, string> Headers { get; set; }
        }

        private class TestRequestWithConvertibleHeaders
        {
            [HeaderCollection]
            public CustomHeaderSource Headers { get; set; }
        }

        private class CustomHeaderSource : IHeaderCollectionConvertible
        {
            private readonly string _key;
            private readonly string _value;

            public CustomHeaderSource(string key, string value)
            {
                _key = key;
                _value = value;
            }

            public IEnumerable<KeyValuePair<string, string>> ToHeaderCollection()
            {
                yield return new KeyValuePair<string, string>(_key, _value);
            }
        }

        private class TestRequestWithCookie
        {
            [Cookie("session")]
            public string SessionId { get; set; }
        }

        private class TestRequestWithIntCookie
        {
            [Cookie("max-age")]
            public int MaxAge { get; set; }
        }

        private class TestRequestWithMultipleCookies
        {
            [Cookie("session")]
            public string SessionId { get; set; }

            [Cookie("preferences")]
            public string Preferences { get; set; }
        }

        private class TestRequestWithCookieCollection
        {
            [CookieCollection]
            public Dictionary<string, string> Cookies { get; set; }
        }

        private class TestRequestWithCookieCollectionObjectDict
        {
            [CookieCollection]
            public Dictionary<string, object> Cookies { get; set; }
        }

        private class TestRequestWithReadOnlyDictionaryCookies
        {
            [CookieCollection]
            public IReadOnlyDictionary<string, string> Cookies { get; set; }
        }

        private class TestRequestWithConvertibleCookies
        {
            [CookieCollection]
            public CustomCookieSource Cookies { get; set; }
        }

        private class CustomCookieSource : ICookieCollectionConvertible
        {
            private readonly string _key;
            private readonly string _value;

            public CustomCookieSource(string key, string value)
            {
                _key = key;
                _value = value;
            }

            public IEnumerable<Cookie> ToCookieCollection()
            {
                yield return new Cookie(_key, _value);
            }
        }

        private class TestRequestWithMultipleHeadersOnSameProperty
        {
            [Header("X-First")]
            [Header("X-Second")]
            public string Value { get; set; }
        }

        private class TestRequestWithMultipleCookiesOnSameProperty
        {
            [Cookie("cookie-a")]
            [Cookie("cookie-b")]
            public string Value { get; set; }
        }

        private class TestRequestWithCookiePath
        {
            [Cookie("session", Path = "/api")]
            public string SessionId { get; set; }
        }

        private class TestRequestWithCookieDomain
        {
            [Cookie("session", Domain = ".example.com")]
            public string SessionId { get; set; }
        }

        private class TestRequestWithCookiePathAndDomain
        {
            [Cookie("session", Path = "/api", Domain = ".example.com")]
            public string SessionId { get; set; }
        }

        private class TestRequestWithConflictingHeaderAttributes
        {
            [Header("X-Custom")]
            [HeaderCollection]
            public Dictionary<string, string> Headers { get; set; }
        }

        private class TestRequestWithConflictingCookieAttributes
        {
            [Cookie("session")]
            [CookieCollection]
            public Dictionary<string, string> Cookies { get; set; }
        }

        private class TestRequestWithConvertibleCookiesWithPathDomain
        {
            [CookieCollection]
            public CustomCookieSourceWithPathDomain Cookies { get; set; }
        }

        private class CustomCookieSourceWithPathDomain : ICookieCollectionConvertible
        {
            public IEnumerable<Cookie> ToCookieCollection()
            {
                yield return new Cookie("session", "abc123", "/api", ".example.com");
            }
        }

        private class TestRequestWithDateTimeHeader
        {
            [Header("X-Date")]
            public DateTime Date { get; set; }
        }

        private class TestRequestWithDateTimeCookie
        {
            [Cookie("expires")]
            public DateTime Expires { get; set; }
        }

        private class TestRequestWithTimeSpanHeader
        {
            [Header("X-Duration")]
            public TimeSpan Duration { get; set; }
        }
    }
}