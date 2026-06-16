using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace JanusRequest
{
    /// <summary>
    /// Extension methods for HttpApiClient and HttpResponseMessage to provide synchronous operations
    /// and additional HTTP header handling functionality.
    /// </summary>
    public static class HttpClientExtension
    {
        private static readonly string[] RequestLimitHeaderNames =
        {
            "X-RateLimit-Limit",
            "X-Rate-Limit-Limit",
            "RequestLimit",
            "Rate-Limit-Limit"
        };

        private static readonly string[] HttpDateFormats =
        {
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
            "dddd, dd-MMM-yy HH:mm:ss 'GMT'",
            "ddd MMM d HH:mm:ss yyyy"
        };

        #region Sync Request

        /// <summary>
        /// Sends a synchronous GET request to the specified URL without a body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous GET operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="url">The URL to send the GET request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Get<TResponse>(this IHttpApiClient client, string url) where TResponse : class
        => SyncRunner.Run(() => client.GetAsync<TResponse>(url));

        /// <summary>
        /// Sends a synchronous GET request using the specified request information without a body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous GET operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="info">The request information including path, headers, and query parameters.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Get<TResponse>(this IHttpApiClient client, HttpRequestInfo info) where TResponse : class
        => SyncRunner.Run(() => client.GetAsync<TResponse>(info));

        /// <summary>
        /// Sends a synchronous GET request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous GET operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Get<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.GetAsync(body, info));

        /// <summary>
        /// Sends a synchronous GET request with the specified request body to the given URL and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous GET operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Get<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.GetAsync(body, url));

        /// <summary>
        /// Sends a synchronous POST request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous POST operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Post<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.PostAsync(body, info));

        /// <summary>
        /// Sends a synchronous POST request with the specified request body to the given URL and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous POST operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Post<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.PostAsync(body, url));

        /// <summary>
        /// Sends a synchronous PUT request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PUT operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Put<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.PutAsync(body, info));

        /// <summary>
        /// Sends a synchronous PUT request with the specified request body to the given URL and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PUT operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Put<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.PutAsync(body, url));

        /// <summary>
        /// Sends a synchronous DELETE request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous DELETE operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Delete<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.DeleteAsync(body, info));

        /// <summary>
        /// Sends a synchronous DELETE request with the specified request body to the given URL and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous DELETE operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Delete<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.DeleteAsync(body, url));

        /// <summary>
        /// Sends a synchronous PATCH request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PATCH operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Patch<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.PatchAsync(body, info));

        /// <summary>
        /// Sends a synchronous PATCH request with the specified request body to the given URL and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PATCH operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Patch<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.PatchAsync(body, url));

        /// <summary>
        /// Sends a synchronous HTTP request with the specified request body and returns a typed response.
        /// Uses the HTTP method and path configured in the request body's attributes.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this IHttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.SendAsync(body, info));

        /// <summary>
        /// Sends a synchronous HTTP request and returns an untyped response.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the raw response data.</returns>
        public static RestApiResponse SendRequest(this IHttpApiClient client, object body, HttpRequestInfo info = null)
        => SyncRunner.Run(() => client.SendRequestAsync(body, info));

        /// <summary>
        /// Sends a synchronous HTTP request and returns the raw HttpResponseMessage.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>The raw HttpResponseMessage from the request.</returns>
        public static HttpResponseMessage SendWebRequest(this IHttpApiClient client, object body, HttpRequestInfo info = null)
        => SyncRunner.Run(() => client.SendHttpRequestAsync(body, info));

        /// <summary>
        /// Sends a synchronous HTTP request with the specified request information and returns a typed response.
        /// No request body is sent with this method.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="info">The request information including method, path, headers, and query parameters.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this IHttpApiClient client, HttpRequestInfo info) where TResponse : class
        => SyncRunner.Run(() => client.SendAsync<TResponse>(info));

        /// <summary>
        /// Sends a synchronous HTTP request with the specified method, request body, and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="httpMethod">The HTTP method to use (GET, POST, PUT, DELETE, PATCH, etc.).</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this IHttpApiClient client, string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        => SyncRunner.Run(() => client.SendAsync(httpMethod, body, info));

        /// <summary>
        /// Sends a synchronous HTTP request with the specified method, request body, and URL, and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <remarks>
        /// This method blocks the calling thread. It may deadlock in environments with a SynchronizationContext
        /// (e.g., WPF, WinForms, legacy ASP.NET). Prefer the async counterpart when possible.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="httpMethod">The HTTP method to use (GET, POST, PUT, DELETE, PATCH, etc.).</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this IHttpApiClient client, string httpMethod, IRequestResponse<TResponse> body, string url) where TResponse : class
        => SyncRunner.Run(() => client.SendAsync(httpMethod, body, url));

        #endregion

        /// <summary>
        /// Attempts to get a single header value from the HTTP headers collection.
        /// </summary>
        /// <param name="header">The HTTP headers collection to search.</param>
        /// <param name="headerName">The name of the header to retrieve.</param>
        /// <param name="value">When this method returns, contains the first header value if found, null otherwise.</param>
        /// <returns>True if the header was found and has a value, false otherwise.</returns>
        public static bool TryGetValue(this HttpHeaders header, string headerName, out string value)
        {
            if (header.TryGetValues(headerName, out var retryAfterValues))
            {
                value = retryAfterValues.FirstOrDefault();
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Gets the retry-after value from the HTTP response headers per RFC 9110 §10.2.3.
        /// Accepts either a delta-seconds integer or an HTTP-date and returns the wait in seconds (0 if absent or invalid).
        /// </summary>
        public static int GetRetryAfter(this HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValue("Retry-After", out var retryAfterValue) || string.IsNullOrWhiteSpace(retryAfterValue))
                return 0;

            retryAfterValue = retryAfterValue.Trim();

            if (int.TryParse(retryAfterValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                return seconds < 0 ? 0 : seconds;

            if (DateTimeOffset.TryParseExact(retryAfterValue, HttpDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var when)
                || DateTimeOffset.TryParse(retryAfterValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out when))
            {
                var delta = when - DateTimeOffset.UtcNow;
                return delta.TotalSeconds <= 0 ? 0 : (int)Math.Ceiling(delta.TotalSeconds);
            }

            return 0;
        }

        /// <summary>
        /// Gets the request limit value from the HTTP response headers.
        /// Searches X-RateLimit-Limit, X-Rate-Limit-Limit, RequestLimit, and Rate-Limit-Limit; returns 0 if none parseable.
        /// </summary>
        public static int GetRequestLimit(this HttpResponseMessage response)
        {
            for (var i = 0; i < RequestLimitHeaderNames.Length; i++)
                if (response.Headers.TryGetValue(RequestLimitHeaderNames[i], out var limitValue)
                    && int.TryParse(limitValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var limit))
                    return limit;

            return 0;
        }
    }
}