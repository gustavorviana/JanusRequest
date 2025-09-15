using System.Net.Http;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Base interface for HTTP response handlers.
    /// This interface defines the contract for handlers that process HTTP responses
    /// based on specific criteria such as status codes, headers, or content.
    /// </summary>
    public interface IHttpHandlerBase
    {
        /// <summary>
        /// Determines whether this handler can process the given HTTP response.
        /// Implementations should return true if they can handle the response based on
        /// their specific criteria (e.g., status code, headers, content type).
        /// </summary>
        /// <param name="response">The HTTP response to evaluate.</param>
        /// <returns>True if this handler can process the response, false otherwise.</returns>
        bool CanHandle(HttpResponseMessage response);
    }
}