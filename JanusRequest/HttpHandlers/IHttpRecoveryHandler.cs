using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Interface for HTTP recovery handlers that can attempt to recover from failed HTTP requests.
    /// This interface extends IHttpHandlerBase to provide recovery functionality for specific
    /// types of HTTP failures, such as authentication failures, rate limiting, or transient errors.
    /// </summary>
    public interface IHttpRecoveryHandler : IHttpHandlerBase
    {
        /// <summary>
        /// Attempts to recover from a failed HTTP request by performing corrective actions
        /// and potentially retrying the request. Recovery strategies may include refreshing
        /// authentication tokens, waiting for rate limits to reset, or implementing retry logic.
        /// </summary>
        /// <param name="context">
        /// The recovery context containing the original request, failed response, HTTP client,
        /// and cancellation token needed for recovery operations.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous recovery operation.
        /// The task result contains the HTTP response from the recovery attempt,
        /// which may be a successful response or another failed response.
        /// </returns>
        Task<HttpResponseMessage> RecoverAsync(HttpRecoveryContext context);
    }
}