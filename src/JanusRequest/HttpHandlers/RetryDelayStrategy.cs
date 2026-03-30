namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Defines the delay strategy used between retry attempts by <see cref="ThrottleRetryHandler"/>.
    /// </summary>
    public enum RetryDelayStrategy
    {
        /// <summary>
        /// Exponential backoff: <c>baseDelay * 2^(attempt-1)</c>.
        /// Produces delays of 1s, 2s, 4s, 8s, 16s, ...
        /// Best for controlled, predictable back-pressure.
        /// </summary>
        ExponentialBackoff = 0,

        /// <summary>
        /// Exponential backoff with jitter: <c>baseDelay * 2^(attempt-1) * random(0.5, 1.5)</c>.
        /// Applies exponential backoff then randomizes the delay to spread retries and avoid thundering herd.
        /// Best for high-concurrency scenarios where many clients retry simultaneously.
        /// </summary>
        Jitter = 1
    }
}
