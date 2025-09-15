using JanusRequest.Builders;
using System.Collections.Specialized;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Represents HTTP request information including method, path, query parameters, headers, and cookies.
    /// This class encapsulates all the configuration needed to construct an HTTP request and provides
    /// functionality to clone the information with optional method override.
    /// </summary>
    public class HttpRequestInfo
    {
        /// <summary>
        /// Gets or sets the HTTP method for the request (GET, POST, PUT, DELETE, PATCH, etc.).
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the URL path for the request.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the query parameters for the request.
        /// Initialized with a new UrlQueryBuilder instance.
        /// </summary>
        public UrlQueryBuilder Query { get; set; } = new UrlQueryBuilder();

        /// <summary>
        /// Gets or sets the HTTP headers for the request.
        /// Initialized with a new NameValueCollection instance.
        /// </summary>
        public NameValueCollection Headers { get; set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the cookies for the request.
        /// Initialized with a new CookieCollection instance.
        /// </summary>
        public CookieCollection Cookies { get; set; } = new CookieCollection();

        /// <summary>
        /// Creates a shallow copy of the current HttpRequestInfo instance with an optional method override.
        /// All collections (Query, Headers, Cookies) are referenced, not deep copied.
        /// </summary>
        /// <param name="method">The HTTP method to use in the clone. If null, uses the current Method value.</param>
        /// <returns>A new HttpRequestInfo instance with the same configuration as the current instance.</returns>
        public HttpRequestInfo Clone(string method = null)
        {
            return new HttpRequestInfo
            {
                Method = method ?? Method,
                Path = Path,
                Query = Query,
                Headers = Headers,
                Cookies = Cookies
            };
        }
    }
}