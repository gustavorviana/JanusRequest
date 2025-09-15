using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Represents the context for HTTP request recovery operations.
    /// This class encapsulates all the necessary information and functionality needed
    /// to retry or recover from failed HTTP requests, including the original request,
    /// response, HTTP client, and cancellation token.
    /// </summary>
    public class HttpRecoveryContext
    {
        /// <summary>
        /// Gets the HTTP client used for the original request.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Gets the original HTTP request message that needs to be recovered.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Gets the HTTP response message from the failed request.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// Gets the cancellation token for controlling the recovery operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initializes a new instance of the HttpRecoveryContext class.
        /// </summary>
        /// <param name="client">The HTTP client to use for recovery operations.</param>
        /// <param name="request">The original HTTP request that failed.</param>
        /// <param name="response">The HTTP response from the failed request.</param>
        /// <param name="token">The cancellation token for controlling the recovery operation.</param>
        public HttpRecoveryContext(HttpClient client, HttpRequestMessage request, HttpResponseMessage response, CancellationToken token)
        {
            Client = client;
            Request = request;
            Response = response;
            CancellationToken = token;
        }

        /// <summary>
        /// Resends the original HTTP request using the same client and cancellation token.
        /// This method can be used to retry the failed request as part of a recovery strategy.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous resend operation. 
        /// The task result contains the HTTP response from the resent request.
        /// </returns>
        public async Task<HttpResponseMessage> ResendAsync()
        {
            return await Client.SendAsync(Request, CancellationToken);
        }
    }
}