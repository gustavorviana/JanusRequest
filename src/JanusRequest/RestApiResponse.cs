using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace JanusRequest
{
    /// <summary>
    /// Represents a REST API response with strongly-typed data content.
    /// </summary>
    /// <typeparam name="TResponse">The type of the deserialized response data.</typeparam>
    public class RestApiResponse<TResponse> : RestApiResponse
    {
        /// <summary>
        /// Gets the deserialized response data.
        /// </summary>
        public TResponse Data { get; }

        internal RestApiResponse(HttpResponseMessage response, TResponse data, Exception error = null)
            : base(response, error)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Represents a REST API response containing HTTP status information and headers.
    /// Inspect <see cref="IsSuccessStatusCode"/>/<see cref="Status"/> for status checks, or call
    /// <see cref="EnsureSuccessStatusCode"/> to throw the exception built by the configured
    /// <see cref="HttpHandlers.IHttpErrorHandler"/> when the response is not successful.
    /// </summary>
    public class RestApiResponse
    {
        /// <summary>
        /// Gets the HTTP status code of the response.
        /// </summary>
        public HttpStatusCode Status { get; }

        /// <summary>
        /// Gets the HTTP status description (reason phrase) of the response.
        /// </summary>
        public string StatusDescription { get; }

        /// <summary>
        /// True when the status code is in the 2xx range.
        /// </summary>
        public bool IsSuccessStatusCode => Status >= HttpStatusCode.OK && Status <= (HttpStatusCode)299;

        /// <summary>
        /// Gets all HTTP headers from both the response and content headers as a read-only dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> Headers { get; }

        private readonly Exception _error;

        internal RestApiResponse(HttpResponseMessage response, Exception error = null)
        {
            Status = response.StatusCode;
            StatusDescription = response.ReasonPhrase ?? response.StatusCode.ToString();
            Headers = ExtractHeaders(response);
            _error = error;
        }

        /// <summary>
        /// Throws the exception produced by the configured <see cref="HttpHandlers.IHttpErrorHandler"/>
        /// (or <see cref="HttpHandlers.HttpErrorHandler.Default"/> as fallback) when the response is not successful.
        /// On a successful response, this method returns without throwing.
        /// </summary>
        public void EnsureSuccessStatusCode()
        {
            if (IsSuccessStatusCode)
                return;

            if (_error != null)
                throw _error;

            // Defensive fallback: only reachable when a RestApiResponse is constructed manually without an error.
            throw new RequestException($"Response status code does not indicate success: {(int)Status} ({StatusDescription}).", Status, null, Headers);
        }

        /// <summary>
        /// Gets the first value of the specified header, or null if absent.
        /// </summary>
        public string GetHeader(string name)
            => Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;

        /// <summary>
        /// Gets all values of the specified header, or an empty sequence if absent.
        /// </summary>
        public IEnumerable<string> GetHeaders(string name)
            => Headers.TryGetValue(name, out var values) ? values : Enumerable.Empty<string>();

        /// <summary>
        /// Indicates whether the response contains the specified header.
        /// </summary>
        public bool HasHeader(string name) => Headers.ContainsKey(name);

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> ExtractHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in response.Headers)
                headers[header.Key] = header.Value as IReadOnlyList<string> ?? header.Value?.ToList() ?? new List<string>();
            if (response.Content?.Headers != null)
                foreach (var header in response.Content.Headers)
                    headers[header.Key] = header.Value as IReadOnlyList<string> ?? header.Value?.ToList() ?? new List<string>();
            return headers;
        }
    }
}
