using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Simple authenticator that sets the Authorization header on each request.
    /// This class contains no business logic — it only applies the configured scheme and value.
    /// For advanced scenarios (token refresh, OAuth), implement <see cref="IHttpAuthenticator"/> directly.
    /// </summary>
    public class HttpAuthenticator : IHttpAuthenticator
    {
        /// <summary>
        /// Gets or sets the authentication scheme (e.g., "Bearer", "Basic").
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the authentication value (e.g., the token or encoded credentials).
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new HttpAuthenticator with the specified scheme and value.
        /// </summary>
        /// <param name="scheme">The authentication scheme (e.g., "Bearer", "Basic").</param>
        /// <param name="value">The authentication value (e.g., token, encoded credentials).</param>
        public HttpAuthenticator(string scheme, string value)
        {
            Scheme = scheme;
            Value = value;
        }

        /// <summary>
        /// Applies the Authorization header to the request.
        /// </summary>
        public Task AuthenticateAsync(HttpRequestMessage request, HttpClient httpClient)
        {
            if (!string.IsNullOrEmpty(Scheme) && !string.IsNullOrEmpty(Value))
                request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, Value);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns false — this simple authenticator has no re-authentication logic.
        /// Override or implement <see cref="IHttpAuthenticator"/> for token refresh scenarios.
        /// </summary>
        public virtual Task<bool> HandleUnauthorizedAsync(HttpRequestMessage request, HttpResponseMessage response, HttpClient httpClient)
        {
            return Task.FromResult(false);
        }
    }
}
