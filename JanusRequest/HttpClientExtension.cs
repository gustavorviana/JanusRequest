using System;
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
        #region Sync Request

        /// <summary>
        /// Sends a synchronous GET request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous GET operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Get<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.GetAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous POST request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous POST operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Post<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.PostAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous PUT request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PUT operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Put<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.PutAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous DELETE request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous DELETE operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Delete<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.DeleteAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous PATCH request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PATCH operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        [Obsolete("Use Patch instead. This method was incorrectly named and will be removed in version 1.0.4.")]
        public static RestApiResponse<TResponse> Path<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.PatchAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous PATCH request with the specified request body and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous PATCH operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Patch<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.PatchAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous HTTP request with the specified request body and returns a typed response.
        /// Uses the HTTP method and path configured in the request body's attributes.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this HttpApiClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.SendAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous HTTP request and returns an untyped response.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the raw response data.</returns>
        public static RestApiResponse SendRequest(this HttpApiClient client, object body, HttpRequestInfo info = null)
        {
            return client.SendRequestAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous HTTP request and returns the raw HttpResponseMessage.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>The raw HttpResponseMessage from the request.</returns>
        public static HttpResponseMessage SendWebRequest(this HttpApiClient client, object body, HttpRequestInfo info = null)
        {
            return client.SendHttpRequestAsync(body, info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous HTTP request with the specified request information and returns a typed response.
        /// No request body is sent with this method.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="info">The request information including method, path, headers, and query parameters.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this HttpApiClient client, HttpRequestInfo info) where TResponse : class
        {
            return client.SendAsync<TResponse>(info).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a synchronous HTTP request with the specified method, request body, and returns a typed response.
        /// This is a blocking operation that waits for the asynchronous operation to complete.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="client">The HttpApiClient instance to extend.</param>
        /// <param name="httpMethod">The HTTP method to use (GET, POST, PUT, DELETE, PATCH, etc.).</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public static RestApiResponse<TResponse> Send<TResponse>(this HttpApiClient client, string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
        {
            return client.SendAsync(httpMethod, body, info).GetAwaiter().GetResult();
        }

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
        /// Gets the retry-after value from the HTTP response headers.
        /// Looks for the "Retry-After" header and attempts to parse it as an integer representing seconds.
        /// </summary>
        /// <param name="response">The HTTP response to extract the retry-after value from.</param>
        /// <returns>The retry-after value in seconds if found and parseable, 0 otherwise.</returns>
        public static int GetRetryAfter(this HttpResponseMessage response)
        {
            return response.Headers.TryGetValue("Retry-After", out var retryAfterValue) &&
                int.TryParse(retryAfterValue, out var retryAfter) ?
                retryAfter : 0;
        }

        /// <summary>
        /// Gets the request limit value from the HTTP response headers.
        /// Searches for common rate limit header names and returns the first parseable limit value found.
        /// </summary>
        /// <param name="response">The HTTP response to extract the request limit from.</param>
        /// <returns>
        /// The request limit value if found in any of the common rate limit headers, 0 otherwise.
        /// Searches headers: X-RateLimit-Limit, X-Rate-Limit-Limit, RequestLimit, Rate-Limit-Limit.
        /// </returns>
        public static int GetRequestLimit(this HttpResponseMessage response)
        {
            var headerNames = new[]
            {
                "X-RateLimit-Limit",
                "X-Rate-Limit-Limit",
                "RequestLimit",
                "Rate-Limit-Limit"
            };

            foreach (var headerName in headerNames)
                if (response.Headers.TryGetValue(headerName, out var limitValue) && int.TryParse(limitValue, out var limit))
                    return limit;

            return 0;
        }
    }
}