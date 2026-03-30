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
        /// Gets or sets the maximum number of seconds to wait based on the Retry-After header.
        /// If the server requests a delay longer than this value, a <see cref="ThrottlingException"/> is thrown
        /// instead of waiting. Default is 300 seconds (5 minutes).
        /// </summary>
        public int MaxRetryAfterSeconds { get; set; } = 300;

        /// <summary>
        /// Determines whether this handler can process the given HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response to check.</param>
        /// <returns>True if the response has a 429 (Too Many Requests) status code, false otherwise.</returns>
        public bool CanHandle(HttpResponseMessage response) => (int)response.StatusCode == 429;

        /// <summary>
        /// Recovers from a throttling response by waiting for the specified retry period and then resending the request.
        /// The delay duration is determined by the Retry-After header from the original response.
        /// If the delay exceeds <see cref="MaxRetryAfterSeconds"/>, a <see cref="ThrottlingException"/> is thrown.
        /// </summary>
        /// <param name="context">
        /// The recovery context containing the original request, throttling response, HTTP client,
        /// and cancellation token for the recovery operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous recovery operation.
        /// The task result contains the HTTP response from the retry attempt after the delay period.
        /// </returns>
        /// <exception cref="ThrottlingException">Thrown when the Retry-After value exceeds <see cref="MaxRetryAfterSeconds"/>.</exception>
        public async Task<HttpResponseMessage> RecoverAsync(HttpRecoveryContext context)
        {
            var retryAfterSeconds = context.Response.GetRetryAfter();
            context.Response.Dispose();

            if (retryAfterSeconds > MaxRetryAfterSeconds)
                throw new ThrottlingException(retryAfterSeconds, 0);

            await Task.Delay(TimeSpan.FromSeconds(retryAfterSeconds), context.CancellationToken);
            return await context.ResendAsync();
        }
    }
}