using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// HTTP recovery handler for transient error responses that implements
    /// RFC 9110 Retry-After semantics with configurable delay strategies.
    ///
    /// <para><b>Handled status codes (default):</b> 408, 429, 503, 504</para>
    ///
    /// <para><b>Retry-After parsing (RFC 9110 §10.2.3):</b></para>
    /// <list type="bullet">
    ///   <item>Delay-seconds: a non-negative integer (e.g. "120")</item>
    ///   <item>HTTP-date: an IMF-fixdate timestamp (e.g. "Fri, 31 Dec 2025 23:59:59 GMT")</item>
    /// </list>
    ///
    /// <para><b>Delay strategies (configured via <see cref="DelayStrategy"/>):</b></para>
    /// <list type="bullet">
    ///   <item><see cref="RetryDelayStrategy.ExponentialBackoff"/>: <c>baseDelay * 2^(attempt-1)</c></item>
    ///   <item><see cref="RetryDelayStrategy.Jitter"/>: <c>baseDelay * 2^(attempt-1) * random(0.5, 1.5)</c></item>
    /// </list>
    ///
    /// When a Retry-After header is present, its value is used directly as the delay —
    /// no backoff or jitter is applied, respecting the server's explicit instruction.
    /// The delay strategy is only applied when no Retry-After header is present.
    /// </summary>
    public class ThrottleRetryHandler : IHttpRecoveryHandler
    {
        private static readonly string[] HttpDateFormats =
        {
            "r",                                       // IMF-fixdate (preferred)
            "ddd, dd MMM yyyy HH:mm:ss 'GMT'",        // RFC 7231
            "dddd, dd-MMM-yy HH:mm:ss 'GMT'",         // obsolete RFC 850
            "ddd MMM  d HH:mm:ss yyyy",                // ANSI C asctime()
            "ddd MMM dd HH:mm:ss yyyy"                 // ANSI C asctime() variant
        };

        private static readonly HashSet<int> DefaultRetryStatusCodes = new HashSet<int>
        {
            408, // Request Timeout
            429, // Too Many Requests
            503, // Service Unavailable
            504  // Gateway Timeout
        };

        private readonly Random _random;
        private readonly HashSet<int> _retryStatusCodes;

        /// <summary>
        /// Gets the maximum number of retry attempts before giving up.
        /// Default is 3.
        /// </summary>
        public int MaxRetries { get; }

        /// <summary>
        /// Gets the base delay in seconds used when no Retry-After header is present.
        /// Default is 1 second.
        /// </summary>
        public double BaseDelaySeconds { get; }

        /// <summary>
        /// Gets the maximum delay in seconds that can be applied between retries.
        /// This cap prevents excessively long waits. Default is 60 seconds.
        /// </summary>
        public double MaxDelaySeconds { get; }

        /// <summary>
        /// Gets the maximum delay as a <see cref="TimeSpan"/>.
        /// </summary>
        public TimeSpan MaxDelay => TimeSpan.FromSeconds(MaxDelaySeconds);

        /// <summary>
        /// Gets the delay strategy used to compute wait times between retries.
        /// Default is <see cref="RetryDelayStrategy.ExponentialBackoff"/>.
        /// </summary>
        public RetryDelayStrategy DelayStrategy { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ThrottleRetryHandler"/> with default settings
        /// (3 retries, 1s base delay, 60s max delay, exponential backoff, status codes: 408, 429, 503, 504).
        /// </summary>
        public ThrottleRetryHandler()
            : this(maxRetries: 3, baseDelaySeconds: 1.0, maxDelaySeconds: 60.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ThrottleRetryHandler"/> with exponential backoff.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts. Must be at least 1.</param>
        /// <param name="baseDelaySeconds">Base delay in seconds. Must be positive.</param>
        /// <param name="maxDelaySeconds">Maximum delay in seconds between retries. Must be positive.</param>
        public ThrottleRetryHandler(int maxRetries, double baseDelaySeconds, double maxDelaySeconds)
            : this(maxRetries, baseDelaySeconds, maxDelaySeconds, RetryDelayStrategy.ExponentialBackoff)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ThrottleRetryHandler"/> with a specific delay strategy.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts. Must be at least 1.</param>
        /// <param name="baseDelaySeconds">Base delay in seconds. Must be positive.</param>
        /// <param name="maxDelaySeconds">Maximum delay in seconds between retries. Must be positive.</param>
        /// <param name="delayStrategy">The delay strategy to use between retries.</param>
        public ThrottleRetryHandler(int maxRetries, double baseDelaySeconds, double maxDelaySeconds, RetryDelayStrategy delayStrategy)
            : this(maxRetries, baseDelaySeconds, maxDelaySeconds, delayStrategy, DefaultRetryStatusCodes, new Random())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ThrottleRetryHandler"/> with custom status codes.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts. Must be at least 1.</param>
        /// <param name="baseDelaySeconds">Base delay in seconds. Must be positive.</param>
        /// <param name="maxDelaySeconds">Maximum delay in seconds between retries. Must be positive.</param>
        /// <param name="delayStrategy">The delay strategy to use between retries.</param>
        /// <param name="retryStatusCodes">The set of HTTP status codes that should trigger a retry.</param>
        public ThrottleRetryHandler(int maxRetries, double baseDelaySeconds, double maxDelaySeconds, RetryDelayStrategy delayStrategy, IEnumerable<int> retryStatusCodes)
            : this(maxRetries, baseDelaySeconds, maxDelaySeconds, delayStrategy, retryStatusCodes, new Random())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ThrottleRetryHandler"/> with an explicit <see cref="Random"/> source.
        /// This constructor is intended for testing to produce deterministic jitter.
        /// </summary>
        internal ThrottleRetryHandler(int maxRetries, double baseDelaySeconds, double maxDelaySeconds, RetryDelayStrategy delayStrategy, IEnumerable<int> retryStatusCodes, Random random)
        {
            if (maxRetries < 1)
                throw new ArgumentOutOfRangeException(nameof(maxRetries), "MaxRetries must be at least 1.");
            if (baseDelaySeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(baseDelaySeconds), "BaseDelaySeconds must be positive.");
            if (maxDelaySeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxDelaySeconds), "MaxDelaySeconds must be positive.");

            MaxRetries = maxRetries;
            BaseDelaySeconds = baseDelaySeconds;
            MaxDelaySeconds = maxDelaySeconds;
            DelayStrategy = delayStrategy;
            _retryStatusCodes = new HashSet<int>(retryStatusCodes ?? DefaultRetryStatusCodes);
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Determines whether this handler can process the given HTTP response.
        /// Returns true for configured retryable status codes (default: 408, 429, 503, 504).
        /// </summary>
        public bool CanHandle(HttpResponseMessage response) => _retryStatusCodes.Contains((int)response.StatusCode);

        /// <summary>
        /// Attempts to recover from a retryable response by retrying with the configured delay strategy.
        /// Respects the Retry-After header when present (RFC 9110 §10.2.3).
        /// </summary>
        public async Task<HttpResponseMessage> RecoverAsync(HttpRecoveryContext context)
        {
            var response = context.Response;
            int attempt = 0;

            while (attempt < MaxRetries)
            {
                attempt++;

                var delay = ComputeDelay(response, attempt);

                response.Dispose();

                await Task.Delay(delay, context.CancellationToken);

                response = await context.ResendAsync();

                if (!_retryStatusCodes.Contains((int)response.StatusCode))
                    return response;
            }

            return response;
        }

        /// <summary>
        /// Computes the delay for the given attempt.
        /// If a Retry-After header is present, its value is used directly (no backoff/jitter applied).
        /// Otherwise, the configured <see cref="BaseDelaySeconds"/> is used with the chosen <see cref="DelayStrategy"/>.
        /// The result is capped at <see cref="MaxDelaySeconds"/>.
        /// </summary>
        internal TimeSpan ComputeDelay(HttpResponseMessage response, int attempt)
        {
            double delaySeconds;

            if (TryParseRetryAfter(response, out var retryAfterDelay))
            {
                delaySeconds = retryAfterDelay.TotalSeconds;
            }
            else
            {
                delaySeconds = BaseDelaySeconds * Math.Pow(2, attempt - 1);

                if (DelayStrategy == RetryDelayStrategy.Jitter)
                {
                    var jitter = 0.5 + _random.NextDouble();
                    delaySeconds *= jitter;
                }
            }

            delaySeconds = Math.Min(delaySeconds, MaxDelaySeconds);
            delaySeconds = Math.Max(delaySeconds, 0);

            return TimeSpan.FromSeconds(delaySeconds);
        }

        /// <summary>
        /// Parses the Retry-After header value according to RFC 9110 §10.2.3.
        /// Supports both delay-seconds (non-negative integer) and HTTP-date formats.
        /// </summary>
        internal static bool TryParseRetryAfter(HttpResponseMessage response, out TimeSpan delay)
        {
            delay = TimeSpan.Zero;

            if (!response.Headers.TryGetValue("Retry-After", out var value) || string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();

            // Try delay-seconds first (non-negative integer)
            if (long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
            {
                delay = TimeSpan.FromSeconds(seconds);
                return true;
            }

            // Try HTTP-date (IMF-fixdate and obsolete formats)
            if (DateTimeOffset.TryParseExact(value, HttpDateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
            {
                var diff = date.UtcDateTime - DateTime.UtcNow;
                delay = diff > TimeSpan.Zero ? diff : TimeSpan.Zero;
                return true;
            }

            return false;
        }
    }
}
