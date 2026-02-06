using Microsoft.Extensions.Logging;

namespace JanusRequest.Extensions.DependencyInjection
{
    /// <summary>
    /// Default adapter that bridges IHttpApiClientLogger to Microsoft.Extensions.Logging.
    /// </summary>
    internal sealed class LoggingHttpApiClientLogger : IHttpApiClientLogger
    {
        private readonly ILogger<HttpApiClient> _logger;
        private readonly HttpApiClientSettings _settings;

        public LoggingHttpApiClientLogger(ILogger<HttpApiClient> logger, HttpApiClientSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void LogRequest(HttpRequestMessage request)
        {
            if (request == null) return;

            _logger.LogDebug(
                "Sending HTTP {Method} {Url}",
                request.Method?.Method,
                request.RequestUri?.ToString());
        }

        public void LogResponse(HttpResponseMessage response)
        {
            if (response == null) return;

            _logger.LogDebug(
                "Received HTTP {StatusCode} from {Url}",
                (int)response.StatusCode,
                response.RequestMessage?.RequestUri?.ToString());
        }

        public void LogError(Exception exception, HttpRequestMessage request, HttpResponseMessage? response)
        {
            if (exception == null) return;

            var url = response?.RequestMessage?.RequestUri?.ToString();
            var statusCode = response != null ? (int)response.StatusCode : (int?)null;

            _logger.LogError(
                exception,
                "Error during HTTP request {Method} {Url} (StatusCode: {StatusCode})",
                request?.Method?.Method,
                url,
                statusCode);

            if (!_settings.LogResponseHeadersOnError)
                return;

            var headers = BuildHeadersToLog(exception, response);

            if (string.IsNullOrEmpty(headers))
                return;

            _logger.LogError(
                "HTTP response headers for {Method} {Url}: {Headers}",
                request?.Method?.Method,
                url,
                headers);
        }

        private static string BuildHeadersToLog(Exception exception, HttpResponseMessage? response)
        {
            if (exception is RequestException reqEx)
            {
                var exceptionHeaders = reqEx
                    .Headers
                    .Select(h => $"{h.Key}={string.Join(",", h.Value)}");

                return string.Join("; ", exceptionHeaders);
            }

            if (response == null)
                return string.Empty;

            var responseHeaders = response.Headers
                    .Concat(response.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                    .Select(h => $"{h.Key}={string.Join(",", h.Value)}");

            return string.Join("; ", responseHeaders);
        }
    }
}

