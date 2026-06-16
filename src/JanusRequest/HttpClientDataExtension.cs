namespace JanusRequest
{
    /// <summary>
    /// Synchronous extension methods for <see cref="IHttpApiDataClient"/>.
    /// Each method offloads its async counterpart to the thread pool via <see cref="SyncRunner"/>,
    /// which avoids the deadlock pattern triggered by <c>.GetAwaiter().GetResult()</c> on a captured
    /// <see cref="System.Threading.SynchronizationContext"/> (WPF, WinForms, legacy ASP.NET).
    /// Prefer the async counterparts when possible.
    /// </summary>
    public static class HttpClientDataExtension
    {
        #region GET

        /// <summary>
        /// Synchronous GET. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, string url) where TResponse : class
            => SyncRunner.Run(() => client.GetDataAsync<TResponse>(url));

        /// <summary>
        /// Synchronous GET using request info. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, HttpRequestInfo info) where TResponse : class
            => SyncRunner.Run(() => client.GetDataAsync<TResponse>(info));

        /// <summary>
        /// Synchronous GET with a request body. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.GetDataAsync(body, info));

        /// <summary>
        /// Synchronous GET with a request body and explicit URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse GetData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.GetDataAsync(body, url));

        #endregion

        #region POST

        /// <summary>
        /// Synchronous POST. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PostData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.PostDataAsync(body, info));

        /// <summary>
        /// Synchronous POST with explicit URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PostData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.PostDataAsync(body, url));

        #endregion

        #region PUT

        /// <summary>
        /// Synchronous PUT. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PutData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.PutDataAsync(body, info));

        /// <summary>
        /// Synchronous PUT with explicit URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PutData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.PutDataAsync(body, url));

        #endregion

        #region DELETE

        /// <summary>
        /// Synchronous DELETE. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse DeleteData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.DeleteDataAsync(body, info));

        /// <summary>
        /// Synchronous DELETE with explicit URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse DeleteData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.DeleteDataAsync(body, url));

        #endregion

        #region PATCH

        /// <summary>
        /// Synchronous PATCH. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PatchData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.PatchDataAsync(body, info));

        /// <summary>
        /// Synchronous PATCH with explicit URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse PatchData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.PatchDataAsync(body, url));

        #endregion

        #region Send

        /// <summary>
        /// Synchronous Send with explicit method. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.SendDataAsync(httpMethod, body, info));

        /// <summary>
        /// Synchronous Send with explicit method and URL. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, string httpMethod, IRequestResponse<TResponse> body, string url) where TResponse : class
            => SyncRunner.Run(() => client.SendDataAsync(httpMethod, body, url));

        /// <summary>
        /// Synchronous Send with body, path and method. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, string path, string method = "GET") where TResponse : class
            => SyncRunner.Run(() => client.SendDataAsync(body, path, method));

        /// <summary>
        /// Synchronous Send using attributes from the body. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, IRequestResponse<TResponse> body, HttpRequestInfo info = null) where TResponse : class
            => SyncRunner.Run(() => client.SendDataAsync(body, info));

        /// <summary>
        /// Synchronous Send using request info only. Throws <see cref="ProblemDetailsException"/> for RFC 9457 bodies, or <see cref="RequestException"/> otherwise.
        /// </summary>
        public static TResponse SendData<TResponse>(this IHttpApiDataClient client, HttpRequestInfo info) where TResponse : class
            => SyncRunner.Run(() => client.SendDataAsync<TResponse>(info));

        #endregion
    }
}
