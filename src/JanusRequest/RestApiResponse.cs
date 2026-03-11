using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace JanusRequest
{
    /// <summary>
    /// Represents a REST API response with strongly-typed data content.
    /// This class extends RestApiResponse to include deserialized response data of the specified type,
    /// providing convenient access to both HTTP response metadata and the parsed response content.
    /// </summary>
    /// <typeparam name="TResponse">The type of the deserialized response data.</typeparam>
    public class RestApiResponse<TResponse> : RestApiResponse
    {
        /// <summary>
        /// Gets the deserialized response data.
        /// </summary>
        public TResponse Data { get; }

        /// <summary>
        /// Initializes a new instance of the RestApiResponse class with HTTP response metadata and deserialized data.
        /// </summary>
        /// <param name="response">The HTTP response message containing status, headers, and metadata.</param>
        /// <param name="data">The deserialized response data of type TResponse.</param>
        internal RestApiResponse(HttpResponseMessage response, TResponse data) : base(response)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Represents a REST API response containing HTTP status information and headers.
    /// This class provides a convenient wrapper around HTTP response data, extracting and
    /// organizing status codes, descriptions, and headers for easy access.
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
        /// Gets all HTTP headers from both the response and content headers as a dictionary.
        /// </summary>
        public Dictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// Initializes a new instance of the RestApiResponse class from an HTTP response message.
        /// Extracts status information and headers from both response and content headers.
        /// </summary>
        /// <param name="response">The HTTP response message to extract information from.</param>
        internal RestApiResponse(HttpResponseMessage response)
        {
            Status = response.StatusCode;
            StatusDescription = response.ReasonPhrase ?? response.StatusCode.ToString();
            Headers = ExtractHeaders(response);
        }

        /// <summary>
        /// Gets the first value of the specified header.
        /// </summary>
        /// <param name="name">The name of the header to retrieve.</param>
        /// <returns>The first value of the header if found, null otherwise.</returns>
        public string GetHeader(string name)
        {
            return Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
        }

        /// <summary>
        /// Gets all values of the specified header.
        /// </summary>
        /// <param name="name">The name of the header to retrieve.</param>
        /// <returns>An enumerable of header values if found, empty enumerable otherwise.</returns>
        public IEnumerable<string> GetHeaders(string name)
        {
            return Headers.TryGetValue(name, out var values) ? values : Enumerable.Empty<string>();
        }

        /// <summary>
        /// Determines whether the response contains the specified header.
        /// </summary>
        /// <param name="name">The name of the header to check for.</param>
        /// <returns>True if the header exists, false otherwise.</returns>
        public bool HasHeader(string name)
        {
            return Headers.ContainsKey(name);
        }

        private static Dictionary<string, IEnumerable<string>> ExtractHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, IEnumerable<string>>();
            foreach (var header in response.Headers)
                headers[header.Key] = header.Value;
            if (response.Content?.Headers != null)
                foreach (var header in response.Content.Headers)
                    headers[header.Key] = header.Value;
            return headers;
        }
    }
}