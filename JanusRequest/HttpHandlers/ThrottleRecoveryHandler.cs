using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// HTTP recovery handler for throttling responses (HTTP 429 Too Many Requests).
    /// This handler implements automatic retry logic with delay based on the Retry-After header
    /// to handle rate limiting scenarios gracefully.
    /// </summary>
    public class ThrottleRecoveryHandler : IHttpRecoveryHandler
    {
        /// <summary>
        /// Determines whether this handler can process the given HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response to check.</param>
        /// <returns>True if the response has a 429 (Too Many Requests) status code, false otherwise.</returns>
        public bool CanHandle(HttpResponseMessage response) => (int)response.StatusCode == 429;

        /// <summary>
        /// Recovers from a throttling response by waiting for the specified retry period and then resending the request.
        /// The delay duration is determined by the Retry-After header from the original response.
        /// </summary>
        /// <param name="context">
        /// The recovery context containing the original request, throttling response, HTTP client,
        /// and cancellation token for the recovery operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous recovery operation.
        /// The task result contains the HTTP response from the retry attempt after the delay period.
        /// </returns>
        public async Task<HttpResponseMessage> RecoverAsync(HttpRecoveryContext context)
        {
            var retryAfterSeconds = context.Response.GetRetryAfter();
            await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds), context.CancellationToken);
            return await context.Client.SendAsync(context.Request, context.CancellationToken);
        }
    }
}