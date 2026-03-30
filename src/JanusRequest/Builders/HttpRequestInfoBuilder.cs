using JanusRequest.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        private readonly NameValueCollection _headers = new NameValueCollection();
        private readonly Dictionary<string, Cookie> _cookies = new Dictionary<string, Cookie>();
        private readonly HttpApiClientSettings _settings;
        private object _request = null;
        private string _pathTemplate;
        private readonly TimeSpan? _timeout;
        private readonly IHttpAuthenticator _authenticator;

        /// <summary>
        /// When true, only properties explicitly marked with <see cref="QueryArgAttribute"/>
        /// are included in the query string. Unmarked properties are ignored regardless of the HTTP method.
        /// </summary>
        public bool StrictQueryArgs { get; set; }

        /// <summary>
        /// Gets the HTTP method that will be used for the request.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Initializes a new instance of the HttpRequestInfoBuilder class.
        /// </summary>
        /// <param name="settings">The HTTP API client settings to use. If null, default settings will be used.</param>
        public HttpRequestInfoBuilder(HttpApiClientSettings settings = null)
        {
            _settings = settings ?? HttpApiClientSettings.Default;
        }

        /// <summary>
        /// Initializes a new instance of the HttpRequestInfoBuilder class with existing HTTP request information.
        /// </summary>
        /// <param name="info">The existing HTTP request information to initialize from.</param>
        /// <param name="settings">The HTTP API client settings to use. If null, default settings will be used.</param>
        public HttpRequestInfoBuilder(HttpRequestInfo info, HttpApiClientSettings settings = null)
        {
            _settings = settings ?? HttpApiClientSettings.Default;
            _pathTemplate = info.Path;
            _timeout = info.Timeout;
            _authenticator = info.Authenticator;

            Method = info.Method;
            StrictQueryArgs = info.CanAddBody();

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

            ApplyHeaderAttributes(request, type);
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
                Timeout = _timeout,
                Authenticator = _authenticator,
            };
        }

        private void ApplyHeaderAttributes(object request, Type type)
        {
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (ApplyHeader(property, request))
                    continue;

                ApplyCookies(property, request);
            }
        }

        private bool ApplyHeader(PropertyInfo property, object request)
        {
            var collectionAttr = property.GetCustomAttribute<HeaderCollectionAttribute>();
            var headerAttrs = property.GetCustomAttributes<HeaderAttribute>()?.ToArray();
            var hasHeaderAttributes = headerAttrs != null && headerAttrs.Length > 0;

            if (collectionAttr != null && hasHeaderAttributes)
                throw new InvalidOperationException(
                    $"Property '{property.Name}' cannot have both [Header] and [HeaderCollection] attributes.");

            if (collectionAttr != null)
            {
                var value = property.GetValue(request);
                if (value != null)
                    ApplyHeaderCollection(value);

                return true;
            }

            if (!hasHeaderAttributes)
                return false;

            foreach (var headerAttr in headerAttrs)
            {
                var value = property.GetValue(request);
                if (value != null)
                    _headers[headerAttr.Name] = _settings.ContentToString(value);
            }

            return true;
        }

        private bool ApplyCookies(PropertyInfo property, object request)
        {
            var cookieCollectionAttr = property.GetCustomAttribute<CookieCollectionAttribute>();
            var cookieAttrs = property.GetCustomAttributes<CookieAttribute>()?.ToArray();
            var hasCookieAttributes = cookieAttrs != null && cookieAttrs.Length > 0;

            if (cookieCollectionAttr != null && hasCookieAttributes)
                throw new InvalidOperationException(
                    $"Property '{property.Name}' cannot have both [Cookie] and [CookieCollection] attributes.");

            if (cookieCollectionAttr != null)
            {
                var value = property.GetValue(request);
                if (value != null)
                    ApplyCookieCollection(value);

                return true;
            }

            if (!hasCookieAttributes)
                return false;

            foreach (var cookieAttr in cookieAttrs)
            {
                var value = property.GetValue(request);
                if (value != null)
                {
                    var cookie = new Cookie(cookieAttr.Name, _settings.ContentToString(value));
                    if (!string.IsNullOrEmpty(cookieAttr.Path))
                        cookie.Path = cookieAttr.Path;

                    if (!string.IsNullOrEmpty(cookieAttr.Domain))
                        cookie.Domain = cookieAttr.Domain;

                    _cookies[cookieAttr.Name] = cookie;
                }
            }

            return true;
        }

        private void ApplyHeaderCollection(object value)
        {
            if (value is IHeaderCollectionConvertible convertible)
            {
                foreach (var kvp in convertible.ToHeaderCollection())
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                        _headers[kvp.Key] = kvp.Value ?? string.Empty;
                }
                return;
            }

            if (value is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    var key = entry.Key?.ToString();
                    if (!string.IsNullOrEmpty(key))
                        _headers[key] = entry.Value?.ToString() ?? string.Empty;
                }
                return;
            }

            if (value is IEnumerable<KeyValuePair<string, string>> stringPairs)
            {
                foreach (var kvp in stringPairs)
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                        _headers[kvp.Key] = kvp.Value ?? string.Empty;
                }
                return;
            }

            if (value is IEnumerable<KeyValuePair<string, object>> objectPairs)
            {
                foreach (var kvp in objectPairs)
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                        _headers[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
        }

        private void ApplyCookieCollection(object value)
        {
            if (value is ICookieCollectionConvertible convertible)
            {
                foreach (var cookie in convertible.ToCookieCollection())
                {
                    if (!string.IsNullOrEmpty(cookie.Name))
                        _cookies[cookie.Name] = cookie;
                }
                return;
            }

            if (value is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    var key = entry.Key?.ToString();
                    if (!string.IsNullOrEmpty(key))
                        _cookies[key] = new Cookie(key, entry.Value?.ToString() ?? string.Empty);
                }
                return;
            }

            if (value is IEnumerable<KeyValuePair<string, string>> stringPairs)
            {
                foreach (var kvp in stringPairs)
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                        _cookies[kvp.Key] = new Cookie(kvp.Key, kvp.Value ?? string.Empty);
                }
                return;
            }

            if (value is IEnumerable<KeyValuePair<string, object>> objectPairs)
            {
                foreach (var kvp in objectPairs)
                {
                    if (!string.IsNullOrEmpty(kvp.Key))
                        _cookies[kvp.Key] = new Cookie(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
                }
            }
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
            return new UrlBuilder(_pathTemplate, _settings).Build(_request);
        }

        private UrlQueryBuilder BuildQuery(string method)
        {
            return new UrlQueryBuilder(_settings)
                .Merge(_query)
                .Add(_request, StrictQueryArgs || !IsNonStandardBodyMethods(method));
        }

        private static bool IsNonStandardBodyMethods(string method)
        {
            return method.Equals("GET", StringComparison.OrdinalIgnoreCase) || method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
        }
    }
}