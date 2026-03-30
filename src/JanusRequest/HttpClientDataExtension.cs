namespace JanusRequest
{
    /// <summary>
    /// Extension methods for IHttpApiDataClient providing synchronous operations.
    /// These methods block the calling thread and may deadlock in environments
    /// with a SynchronizationContext (e.g., WPF, WinForms, legacy ASP.NET).
    /// Prefer the async counterparts when possible.
    /// </summary>
    public static class HttpClientDataExtension
    {
        #region GET

        /// <summary>
        /// Sends a synchronous GET request and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, string url) where TResponse : class
            => client.GetDataAsync<TResponse>(url).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous GET request using the specified request info and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, HttpRequestInfo info) where TResponse : class
            => client.GetDataAsync<TResponse>(info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous GET request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.GetDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous GET request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.GetDataAsync(body, url).GetAwaiter().GetResult();

        #endregion

        #region POST

        /// <summary>
        /// Sends a synchronous POST request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PostData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.PostDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous POST request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PostData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.PostDataAsync(body, url).GetAwaiter().GetResult();

        #endregion

        #region PUT

        /// <summary>
        /// Sends a synchronous PUT request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PutData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.PutDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous PUT request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PutData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.PutDataAsync(body, url).GetAwaiter().GetResult();

        #endregion

        #region DELETE

        /// <summary>
        /// Sends a synchronous DELETE request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse DeleteData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.DeleteDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous DELETE request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse DeleteData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.DeleteDataAsync(body, url).GetAwaiter().GetResult();

        #endregion

        #region PATCH

        /// <summary>
        /// Sends a synchronous PATCH request with the specified body and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PatchData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.PatchDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous PATCH request with the specified body to the given URL and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse PatchData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.PatchDataAsync(body, url).GetAwaiter().GetResult();

        #endregion

        #region Send

        /// <summary>
        /// Sends a synchronous HTTP request with the specified method and body, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.SendDataAsync(httpMethod, body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous HTTP request with the specified method, body, and URL, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, string httpMethod, IRequestResponse<TResponse> body, string url) where TResponse : class
            => client.SendDataAsync(httpMethod, body, url).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous HTTP request with the specified body, path, and method, returning the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string path, string method = "GET") where TResponse : class
            => client.SendDataAsync(body, path, method).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous HTTP request using the body's attributes and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => client.SendDataAsync(body, info).GetAwaiter().GetResult();

        /// <summary>
        /// Sends a synchronous HTTP request using the specified request info (no body) and returns the deserialized response data.
        /// Throws <see cref="RequestException"/> on non-2xx status.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, HttpRequestInfo info) where TResponse : class
            => client.SendDataAsync<TResponse>(info).GetAwaiter().GetResult();

        #endregion
    }
}
