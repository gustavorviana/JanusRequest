using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Provides HTTP operation methods (GET, POST, PUT, DELETE, PATCH, Send) returning
    /// typed <see cref="RestApiResponse{TResponse}"/> or raw <see cref="HttpResponseMessage"/>.
    /// </summary>
    public interface IHttpApiOperations : IDisposable
    {
        #region GET

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

        #endregion

        #region POST

        /// <summary>
        /// Sends a POST request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PostAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a POST request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PostAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region PUT

        /// <summary>
        /// Sends a PUT request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PutAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PUT request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PutAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region DELETE

        /// <summary>
        /// Sends a DELETE request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a DELETE request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region PATCH

        /// <summary>
        /// Sends a PATCH request with the specified request body and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class;

        /// <summary>
        /// Sends a PATCH request with the specified request body to the given URL and returns a typed response.
        /// </summary>
        Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(IRequestResponse<TResponse> body, string url, CancellationToken cancellationToken = default) where TResponse : class;

        #endregion

        #region Send

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
