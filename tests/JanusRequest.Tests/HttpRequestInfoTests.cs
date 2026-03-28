using JanusRequest.Builders;
using System.Collections.Specialized;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpRequestInfoTests
    {
        [Fact]
        public void AllowNonStandardBody_DefaultsToNone()
        {
            var info = new HttpRequestInfo();
            Assert.Equal(NonStandardBodyMethods.None, info.AllowNonStandardBody);
        }

        [Fact]
        public void Query_DefaultsToNewInstance()
        {
            var info = new HttpRequestInfo();
            Assert.NotNull(info.Query);
        }

        [Fact]
        public void Headers_DefaultsToNewInstance()
        {
            var info = new HttpRequestInfo();
            Assert.NotNull(info.Headers);
        }

        [Fact]
        public void Cookies_DefaultsToNewInstance()
        {
            var info = new HttpRequestInfo();
            Assert.NotNull(info.Cookies);
        }

        #region CanAddBody Tests

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        public void CanAddBody_WithStandardBodyMethods_ReturnsTrue(string method)
        {
            var info = new HttpRequestInfo { Method = method };
            Assert.True(info.CanAddBody());
        }

        [Theory]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        public void CanAddBody_WithHeadOrOptions_ReturnsFalse(string method)
        {
            var info = new HttpRequestInfo
            {
                Method = method,
                AllowNonStandardBody = NonStandardBodyMethods.All
            };
            Assert.False(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_GET_WithNone_ReturnsFalse()
        {
            var info = new HttpRequestInfo
            {
                Method = "GET",
                AllowNonStandardBody = NonStandardBodyMethods.None
            };
            Assert.False(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_GET_WithGetFlag_ReturnsTrue()
        {
            var info = new HttpRequestInfo
            {
                Method = "GET",
                AllowNonStandardBody = NonStandardBodyMethods.Get
            };
            Assert.True(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_GET_WithAll_ReturnsTrue()
        {
            var info = new HttpRequestInfo
            {
                Method = "GET",
                AllowNonStandardBody = NonStandardBodyMethods.All
            };
            Assert.True(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_DELETE_WithNone_ReturnsFalse()
        {
            var info = new HttpRequestInfo
            {
                Method = "DELETE",
                AllowNonStandardBody = NonStandardBodyMethods.None
            };
            Assert.False(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_DELETE_WithDeleteFlag_ReturnsTrue()
        {
            var info = new HttpRequestInfo
            {
                Method = "DELETE",
                AllowNonStandardBody = NonStandardBodyMethods.Delete
            };
            Assert.True(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_DELETE_WithAll_ReturnsTrue()
        {
            var info = new HttpRequestInfo
            {
                Method = "DELETE",
                AllowNonStandardBody = NonStandardBodyMethods.All
            };
            Assert.True(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_GET_WithDeleteFlag_ReturnsFalse()
        {
            var info = new HttpRequestInfo
            {
                Method = "GET",
                AllowNonStandardBody = NonStandardBodyMethods.Delete
            };
            Assert.False(info.CanAddBody());
        }

        [Fact]
        public void CanAddBody_DELETE_WithGetFlag_ReturnsFalse()
        {
            var info = new HttpRequestInfo
            {
                Method = "DELETE",
                AllowNonStandardBody = NonStandardBodyMethods.Get
            };
            Assert.False(info.CanAddBody());
        }

        [Theory]
        [InlineData("get")]
        [InlineData("Get")]
        [InlineData("GET")]
        public void CanAddBody_GET_IsCaseInsensitive(string method)
        {
            var info = new HttpRequestInfo
            {
                Method = method,
                AllowNonStandardBody = NonStandardBodyMethods.Get
            };
            Assert.True(info.CanAddBody());
        }

        [Theory]
        [InlineData("head")]
        [InlineData("Head")]
        [InlineData("HEAD")]
        public void CanAddBody_HEAD_IsCaseInsensitive(string method)
        {
            var info = new HttpRequestInfo { Method = method };
            Assert.False(info.CanAddBody());
        }

        #endregion

        #region Clone Tests

        [Fact]
        public void Clone_CopiesAllProperties()
        {
            var query = new UrlQueryBuilder().Set("key", "value");
            var headers = new NameValueCollection { ["X-Test"] = "val" };
            var cookies = new CookieCollection { new Cookie("c", "v") };

            var info = new HttpRequestInfo
            {
                Method = "POST",
                Path = "/api/test",
                Query = query,
                Headers = headers,
                Cookies = cookies
            };

            var clone = info.Clone();

            Assert.Equal("POST", clone.Method);
            Assert.Equal("/api/test", clone.Path);
            Assert.NotSame(query, clone.Query);
            Assert.NotSame(headers, clone.Headers);
            Assert.NotSame(cookies, clone.Cookies);
        }

        [Fact]
        public void Clone_WithMethodOverride_UsesNewMethod()
        {
            var info = new HttpRequestInfo { Method = "POST" };

            var clone = info.Clone("GET");

            Assert.Equal("GET", clone.Method);
        }

        [Fact]
        public void Clone_WithoutMethodOverride_UsesOriginalMethod()
        {
            var info = new HttpRequestInfo { Method = "PUT" };

            var clone = info.Clone();

            Assert.Equal("PUT", clone.Method);
        }

        [Fact]
        public void Clone_DoesNotMutateOriginal()
        {
            var info = new HttpRequestInfo { Method = "POST", Path = "/original" };

            var clone = info.Clone("DELETE");
            clone.Path = "/modified";

            Assert.Equal("POST", info.Method);
            Assert.Equal("/original", info.Path);
        }

        #endregion
    }
}
