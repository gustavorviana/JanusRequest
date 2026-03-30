using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Authenticator that sets a custom header (e.g., X-API-Key) on each request.
    /// For Authorization header schemes (Bearer, Basic), use <see cref="AuthorizationHeaderAuthenticator"/> instead.
    /// </summary>
    public class ApiKeyAuthenticator : IHttpAuthenticator
    {
        /// <summary>
        /// Gets or sets the API key value.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the header name for the API key. Defaults to "X-API-Key".
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// Creates a new ApiKeyAuthenticator with the specified API key and header name.
        /// </summary>
        /// <param name="apiKey">The API key value.</param>
        /// <param name="headerName">The header name for the API key. Defaults to "X-API-Key".</param>
        public ApiKeyAuthenticator(string apiKey, string headerName = "X-API-Key")
        {
            ApiKey = apiKey;
            HeaderName = headerName;
        }

        /// <summary>
        /// Applies the API key header to the request.
        /// </summary>
        public Task AuthenticateAsync(HttpRequestMessage request, HttpClient httpClient)
        {
            if (!string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(HeaderName))
            {
                request.Headers.Remove(HeaderName);
                request.Headers.Add(HeaderName, ApiKey);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns false — this simple authenticator has no re-authentication logic.
        /// </summary>
        public virtual Task<bool> HandleUnauthorizedAsync(HttpRequestMessage request, HttpResponseMessage response, HttpClient httpClient)
        {
            return Task.FromResult(false);
        }
    }
}
