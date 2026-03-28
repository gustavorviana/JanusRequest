using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Abstraction for making REST API requests with automatic serialization, deserialization,
    /// error handling, and recovery mechanisms. Inject this interface to enable unit testing
    /// of code that depends on HttpApiClient.
    /// </summary>
    public interface IHttpApiClient : IHttpApiClientBase
    {
        #region Authentication

        /// <summary>
        /// Sets basic authentication using username and password.
        /// </summary>
        new IHttpApiClient SetBasicAuthentication(string username, string password);

        /// <summary>
        /// Sets bearer token authentication.
        /// </summary>
        new IHttpApiClient SetBearerAuthentication(string token);

        /// <summary>
        /// Sets API key authentication using a custom header.
        /// </summary>
        new IHttpApiClient SetApiKeyAuthentication(string apiKey, string headerName = "X-API-Key");

        /// <summary>
        /// Sets custom authentication with the specified scheme and value.
        /// </summary>
        new IHttpApiClient SetAuthentication(string scheme, string value);

        /// <summary>
        /// Clears any existing authentication configuration.
        /// </summary>
        new IHttpApiClient ClearAuthentication();

        #endregion

        #region Async Request

        /// <summary>
        /// Sends a GET request to the specified URL without a body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> GetAsync<TResponse>(string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request using the specified HttpRequestInfo without a body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> GetAsync<TResponse>(HttpRequestInfo info, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> GetAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> GetAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a POST request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PostAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a POST request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PostAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PUT request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PutAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PUT request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PutAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a DELETE request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a DELETE request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PATCH request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PATCH request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified method, request body, and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> SendAsync<TResponse>(string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified HTTP method, request body, and string URL.
        /// </summary>
        Task<RestApiResponse<TResponse>> SendAsync<TResponse>(string httpMethod, IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified URL, request body, and HTTP method.
        /// </summary>
        Task<RestApiResponse<TResponse>> SendAsync<TResponse>(IRequestResponse<TResponse> body, string path, string method = "GET", CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified request body and returns a typed response.
        /// Uses the HTTP method and path configured in the request body's attributes.
        /// </summary>
        Task<RestApiResponse<TResponse>> SendAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified request information and returns a typed response.
        /// No request body is sent with this method.
        /// </summary>
        Task<RestApiResponse<TResponse>> SendAsync<TResponse>(HttpRequestInfo info, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request and returns an untyped response.
        /// </summary>
        Task<RestApiResponse> SendRequestAsync(object body, HttpRequestInfo info = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an HTTP request and returns the raw HttpResponseMessage.
        /// </summary>
        Task<HttpResponseMessage> SendHttpRequestAsync(object body, HttpRequestInfo info = null, CancellationToken cancellationToken = default);

        #endregion
    }
}
