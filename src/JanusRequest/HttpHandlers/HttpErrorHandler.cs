using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Default <see cref="IHttpErrorHandler"/> that maps unsuccessful HTTP responses (status >= 400)
    /// to exceptions. Returns <see cref="ThrottlingException"/> for 429, <see cref="ProblemDetailsException"/>
    /// when the response body can be parsed as RFC 9457 problem details, and <see cref="RequestException"/>
    /// otherwise.
    /// </summary>
    public class HttpErrorHandler : IHttpErrorHandler
    {
        /// <summary>
        /// Shared stateless instance used as the fallback error mapper when no <see cref="IHttpErrorHandler"/>
        /// is registered on <see cref="HttpApiClientSettings"/>.
        /// </summary>
        public static readonly HttpErrorHandler Default = new HttpErrorHandler();


        /// <summary>
        /// Returns true for HTTP error responses, defined as status code >= 400 (4xx and 5xx).
        /// 1xx and 3xx responses are not considered errors.
        /// </summary>
        public virtual bool CanHandle(HttpResponseMessage response) => (int)response.StatusCode >= 400;

        /// <summary>
        /// Maps an unsuccessful HTTP response to an exception.
        /// </summary>
        /// <param name="response">The HTTP response to map.</param>
        /// <param name="settings">The settings of the originating client, used to access the configured problem details deserializer.</param>
        public virtual async Task<Exception> MapExceptionAsync(HttpResponseMessage response, HttpApiClientSettings settings, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((int)response.StatusCode == 429)
                return OnThrottling(response);

            var headers = Utils.ExtractHeaders(response);
            var body = response.Content != null
                ? await response.Content.ReadAsStringAsync()
                : null;
            var url = response.RequestMessage?.RequestUri?.ToString();

            var problem = await settings.TryParseProblemDetailsAsync(response, body, cancellationToken);
            if (problem != null)
            {
                return new ProblemDetailsException(response.StatusCode, problem, body, headers)
                {
                    Url = url
                };
            }

            return new RequestException(response.StatusCode, body, headers)
            {
                Url = url
            };
        }

        /// <summary>
        /// Handles throttling responses (HTTP 429) by creating a <see cref="ThrottlingException"/>
        /// populated with retry-after and request limit headers when present.
        /// </summary>
        protected virtual Exception OnThrottling(HttpResponseMessage response)
        {
            return new ThrottlingException(response.GetRetryAfter(), response.GetRequestLimit());
        }
    }
}
