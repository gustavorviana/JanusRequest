using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Defines a contract for applying authentication to HTTP requests and handling
    /// 401 Unauthorized responses. Implement this interface to provide custom authentication
    /// strategies such as token refresh, OAuth flows, or dynamic credential lookup.
    /// </summary>
    public interface IHttpAuthenticator
    {
        /// <summary>
        /// Called before each request is sent. Implementations should apply authentication
        /// credentials to the request (e.g., set the Authorization header).
        /// </summary>
        /// <param name="request">The outgoing HTTP request message.</param>
        /// <param name="httpClient">The underlying HttpClient, available for token-refresh calls.</param>
        Task AuthenticateAsync(HttpRequestMessage request, HttpClient httpClient);

        /// <summary>
        /// Called when a 401 Unauthorized response is received. Implementations should
        /// attempt to re-authenticate (e.g., refresh an expired token) and update the
        /// request with new credentials.
        /// </summary>
        /// <param name="request">
        /// The cloned HTTP request message that will be retried if this method returns true.
        /// Implementations should update this request's credentials before returning.
        /// </param>
        /// <param name="response">The 401 response received from the server.</param>
        /// <param name="httpClient">The underlying HttpClient, available for token-refresh calls.</param>
        /// <returns>
        /// True if re-authentication succeeded and the request should be retried once;
        /// false if the request should not be retried and the 401 response should be returned as-is.
        /// </returns>
        Task<bool> HandleUnauthorizedAsync(HttpRequestMessage request, HttpResponseMessage response, HttpClient httpClient);
    }
}
