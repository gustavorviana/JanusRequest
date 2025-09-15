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
    }
}