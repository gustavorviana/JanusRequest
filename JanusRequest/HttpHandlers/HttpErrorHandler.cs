using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// HTTP error handler responsible for processing unsuccessful HTTP responses and mapping them to appropriate exceptions.
    /// This handler provides default error handling behavior for common HTTP status codes including unauthorized access,
    /// throttling (429), and other error responses.
    /// </summary>
    public class HttpErrorHandler : IHttpHandlerBase
    {
        /// <summary>
        /// Determines whether this handler can process the given HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response to check.</param>
        /// <returns>True if the response has an unsuccessful status code, false otherwise.</returns>
        public virtual bool CanHandle(HttpResponseMessage response) => !response.IsSuccessStatusCode;

        /// <summary>
        /// Maps an unsuccessful HTTP response to an appropriate exception.
        /// Provides specific handling for throttling (429) and unauthorized (401) responses,
        /// with a general RequestException for other error status codes.
        /// </summary>
        /// <param name="response">The HTTP response to map to an exception.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains:
        /// - ThrottlingException for 429 status codes
        /// - UnauthorizedAccessException for 401 status codes  
        /// - RequestException for other unsuccessful status codes
        /// </returns>
        public virtual async Task<Exception> MapExceptionAsync(HttpResponseMessage response)
        {
            if ((int)response.StatusCode == 429)
                return OnThrottling(response);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new UnauthorizedAccessException("O servidor recusou as credenciais da API.");

            return new RequestException(response.StatusCode, await response.Content.ReadAsStringAsync())
            {
                Url = response.RequestMessage?.RequestUri?.ToString()
            };
        }

        /// <summary>
        /// Handles throttling responses (HTTP 429) by creating a ThrottlingException.
        /// This method can be overridden to provide custom throttling handling behavior.
        /// </summary>
        /// <param name="response">The HTTP response with 429 status code.</param>
        /// <returns>A ThrottlingException containing retry-after and request limit information from the response headers.</returns>
        protected virtual Exception OnThrottling(HttpResponseMessage response)
        {
            return new ThrottlingException(response.GetRetryAfter(), response.GetRequestLimit());
        }
    }
}