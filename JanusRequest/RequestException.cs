using System;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Exception that represents an HTTP request error with status code and response details.
    /// This exception is thrown when an HTTP request fails and provides access to the
    /// HTTP status code, response content, and request URL for error handling and debugging.
    /// </summary>
    public class RequestException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code returned by the failed request.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the response content from the failed request.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Gets or sets the URL of the request that caused this exception.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Initializes a new instance of the RequestException class with the specified status code and response content.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the failed request.</param>
        /// <param name="response">The response content from the failed request.</param>
        public RequestException(HttpStatusCode statusCode, string response) : this(statusCode)
        {
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the RequestException class with the specified status code.
        /// Creates a default error message based on the status code.
        /// </summary>
        /// <param name="code">The HTTP status code of the failed request.</param>
        public RequestException(HttpStatusCode code) : base($"Error code: {code}")
        {
            StatusCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the RequestException class with a custom error message.
        /// </summary>
        /// <param name="message">The custom error message.</param>
        public RequestException(string message) : base(message)
        {
        }
    }
}