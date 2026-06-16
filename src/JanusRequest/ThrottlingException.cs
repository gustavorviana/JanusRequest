using System;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Exception that represents a throttling error when API rate limits are exceeded.
    /// Thrown when the server responds with HTTP 429 (Too Many Requests).
    /// Inherits from <see cref="RequestException"/> so generic <c>catch (RequestException)</c> handlers receive it.
    /// </summary>
    public class ThrottlingException : RequestException
    {
        /// <summary>
        /// Gets the number of seconds after which the request can be retried.
        /// This value is typically extracted from the "Retry-After" HTTP header.
        /// </summary>
        public int RetryAfter { get; }

        /// <summary>
        /// Gets the maximum number of requests allowed within the rate limit window.
        /// This value is typically extracted from rate limit headers like "X-RateLimit-Limit".
        /// </summary>
        public int RequestLimit { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this throttling error is fatal and should not be retried.
        /// </summary>
        public bool IsFatal { get; set; }

        /// <summary>
        /// Gets the UTC date and time when the request can be retried.
        /// This is calculated as the current UTC time plus the RetryAfter seconds.
        /// </summary>
        public DateTime RetryAt { get; }

        /// <summary>
        /// Initializes a new instance of the ThrottlingException class with retry timing and request limit information.
        /// Uses a default error message about rate limit being reached.
        /// </summary>
        /// <param name="retryAt">The number of seconds after which the request can be retried.</param>
        /// <param name="requestLimit">The maximum number of requests allowed within the rate limit window.</param>
        public ThrottlingException(int retryAt, int requestLimit) : this(retryAt, requestLimit, "The request limit has been reached.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the ThrottlingException class with retry timing, request limit, and custom error message.
        /// </summary>
        /// <param name="retryAt">The number of seconds after which the request can be retried.</param>
        /// <param name="requestLimit">The maximum number of requests allowed within the rate limit window.</param>
        /// <param name="message">The custom error message.</param>
        public ThrottlingException(int retryAt, int requestLimit, string message)
            : base(message, (HttpStatusCode)429)
        {
            RetryAfter = retryAt;
            RequestLimit = requestLimit;
            RetryAt = DateTime.UtcNow.AddSeconds(RetryAfter);
        }
    }
}