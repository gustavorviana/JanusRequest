using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Provides HTTP API methods that return deserialized response data directly,
    /// throwing <see cref="RequestException"/> on non-success status codes.
    /// This interface simplifies common usage patterns where callers only need the response data
    /// and want automatic error handling without inspecting HTTP metadata.
    /// </summary>
    public interface IHttpApiDataClient : IHttpApiClientBase
    {
        #region Authentication

        /// <summary>
        /// Sets basic authentication using username and password.
        /// </summary>
        new IHttpApiDataClient SetBasicAuthentication(string username, string password);

        /// <summary>
        /// Sets bearer token authentication.
        /// </summary>
        new IHttpApiDataClient SetBearerAuthentication(string token);

        /// <summary>
        /// Sets API key authentication using a custom header.
        /// </summary>
        new IHttpApiDataClient SetApiKeyAuthentication(string apiKey, string headerName = "X-API-Key");

        /// <summary>
        /// Sets custom authentication with the specified scheme and value.
        /// </summary>
        new IHttpApiDataClient SetAuthentication(string scheme, string value);

        /// <summary>
        /// Clears any existing authentication configuration.
        /// </summary>
        new IHttpApiDataClient ClearAuthentication();

        #endregion

        #region GET

        /// <summary>
        /// Sends a GET request and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> GetDataAsync<TResponse>(string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request using the specified request info and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> GetDataAsync<TResponse>(HttpRequestInfo info, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> GetDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a GET request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> GetDataAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region POST

        /// <summary>
        /// Sends a POST request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PostDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a POST request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PostDataAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region PUT

        /// <summary>
        /// Sends a PUT request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PutDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PUT request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PutDataAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region DELETE

        /// <summary>
        /// Sends a DELETE request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> DeleteDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a DELETE request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> DeleteDataAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region PATCH

        /// <summary>
        /// Sends a PATCH request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PatchDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PATCH request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> PatchDataAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region Send

        /// <summary>
        /// Sends an HTTP request with the specified method and body, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> SendDataAsync<TResponse>(string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified method, body, and URL, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> SendDataAsync<TResponse>(string httpMethod, IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request with the specified body, path, and method, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> SendDataAsync<TResponse>(IRequestResponse<TResponse> body, string path, string method = "GET", CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request using the body's attributes and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> SendDataAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends an HTTP request using the specified request info (no body) and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> if the response status code is not 2xx.
        /// </summary>
        Task<TResponse> SendDataAsync<TResponse>(HttpRequestInfo info, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion
    }
}
