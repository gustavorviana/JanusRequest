using System;
using System.Net.Http;

namespace JanusRequest
{
    /// <summary>
    /// Abstraction for logging HTTP requests, responses and errors produced by <see cref="HttpApiClient"/>.
    /// This interface lives in the core library to avoid a hard dependency on any specific logging framework.
    /// </summary>
    public interface IHttpApiClientLogger
    {
        /// <summary>
        /// Logs an outgoing HTTP request before it is sent.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        void LogRequest(HttpRequestMessage request);

        /// <summary>
        /// Logs an incoming HTTP response after it is received.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        void LogResponse(HttpResponseMessage response);

        /// <summary>
        /// Logs an error that occurred while sending a request or processing a response.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="request">The HTTP request message associated with the error.</param>
        /// <param name="response">The HTTP response message, if available.</param>
        void LogError(Exception exception, HttpRequestMessage request, HttpResponseMessage response);
    }
}

