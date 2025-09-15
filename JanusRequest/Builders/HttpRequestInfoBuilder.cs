using JanusRequest.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;

namespace JanusRequest.Builders
{
    /// <summary>
    /// Builder class for constructing HTTP request information.
    /// This class provides a fluent interface to configure HTTP requests including
    /// method, path, query parameters, headers, and cookies. It can apply configuration
    /// from request attributes and objects to build complete HTTP request information.
    /// </summary>
    public class HttpRequestInfoBuilder
    {
        private readonly UrlQueryBuilder _query = new UrlQueryBuilder();
        private readonly WebHeaderCollection _headers = new WebHeaderCollection();
        private readonly Dictionary<string, Cookie> _cookies = new Dictionary<string, Cookie>();
        private object _request = null;
        private string _pathTemplate;

        /// <summary>
        /// Gets the HTTP method that will be used for the request.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Initializes a new instance of the HttpRequestInfoBuilder class.
        /// </summary>
        public HttpRequestInfoBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestInfoBuilder class with existing HTTP request information.
        /// </summary>
        /// <param name="info">The existing HTTP request information to initialize from.</param>
        public HttpRequestInfoBuilder(HttpRequestInfo info)
        {
            _pathTemplate = info.Path;
            Method = info.Method;
            AddQuery(info.Query);
            AddHeader(info.Headers);
            AddCookie(info.Cookies);
        }

        /// <summary>
        /// Applies a request object to the builder, extracting configuration from its attributes.
        /// </summary>
        /// <param name="request">The request object to apply. Can be null.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder ApplyRequestObject(object request)
        {
            _request = request;
            if (request == null)
                return this;

            var type = request.GetType();
            if (ReflectionUtils.IsNative(type, false))
                return this;

            return ApplyRequestAttribute(type.GetCustomAttribute<RequestAttribute>());
        }

        /// <summary>
        /// Sets the URL path template for the request.
        /// </summary>
        /// <param name="path">The path template to set. Will be trimmed if not null or empty.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder SetPath(string path)
        {
            path = path?.Trim();
            if (!string.IsNullOrEmpty(path))
                _pathTemplate = path;
            return this;
        }

        /// <summary>
        /// Sets the HTTP method for the request.
        /// </summary>
        /// <param name="method">The HTTP method to set (e.g., GET, POST, PUT, DELETE).</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder SetMethod(string method)
        {
            if (!string.IsNullOrEmpty(method))
                Method = method;
            return this;
        }

        /// <summary>
        /// Adds query parameters from an existing UrlQueryBuilder to the request.
        /// </summary>
        /// <param name="query">The UrlQueryBuilder containing query parameters to add.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder AddQuery(UrlQueryBuilder query)
        {
            _query.AddAll(query);
            return this;
        }

        /// <summary>
        /// Adds headers from a NameValueCollection to the request.
        /// </summary>
        /// <param name="headers">The collection of headers to add.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder AddHeader(NameValueCollection headers)
        {
            foreach (string key in headers.Keys)
                _headers[key] = headers[key];
            return this;
        }

        /// <summary>
        /// Adds cookies from a CookieCollection to the request.
        /// </summary>
        /// <param name="cookies">The collection of cookies to add.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder AddCookie(CookieCollection cookies)
        {
            for (int i = 0; i < cookies.Count; i++)
                AddCookie(cookies[i]);
            return this;
        }

        /// <summary>
        /// Adds a single cookie to the request.
        /// </summary>
        /// <param name="cookie">The cookie to add.</param>
        /// <returns>The current HttpRequestInfoBuilder instance for method chaining.</returns>
        public HttpRequestInfoBuilder AddCookie(Cookie cookie)
        {
            _cookies[cookie.Name] = cookie;
            return this;
        }

        /// <summary>
        /// Builds and returns the final HttpRequestInfo object with all configured settings.
        /// </summary>
        /// <returns>A new HttpRequestInfo instance containing all the configured request information.</returns>
        public HttpRequestInfo Build()
        {
            var cookies = new CookieCollection();
            foreach (var item in _cookies.Values)
                cookies.Add(item);

            var method = Method ?? "GET";
            return new HttpRequestInfo
            {
                Cookies = cookies,
                Headers = _headers,
                Path = BuildPath(),
                Query = BuildQuery(method),
                Method = method,
            };
        }

        private HttpRequestInfoBuilder ApplyRequestAttribute(RequestAttribute attribute)
        {
            if (attribute == null)
                return this;

            if (string.IsNullOrEmpty(Method))
                SetMethod(attribute.Method);

            if (string.IsNullOrEmpty(_pathTemplate))
                SetPath(attribute.Path);

            return this;
        }

        private string BuildPath()
        {
            return new UrlBuilder(_pathTemplate).Build(_request);
        }

        private UrlQueryBuilder BuildQuery(string method)
        {
            return new UrlQueryBuilder()
                .Merge(_query)
                .Add(_request, !method.Equals("GET", StringComparison.OrdinalIgnoreCase));
        }
    }
}