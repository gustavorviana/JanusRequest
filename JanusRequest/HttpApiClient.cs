using JanusRequest.Builders;
using JanusRequest.HttpHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Main HTTP API client for making REST API requests with automatic serialization, deserialization,
    /// error handling, and recovery mechanisms. Provides a fluent interface for configuring authentication,
    /// headers, and request parameters while supporting various content types and response handling strategies.
    /// </summary>
    public class HttpApiClient : IDisposable
    {
        /// <summary>
        /// The default content type string for JSON requests.
        /// </summary>
        public const string JsonContentType = "application/json";

        private HttpApiClientSettings _settings = HttpApiClientSettings.Default;

        /// <summary>
        /// Gets or sets the HTTP API client settings that control serialization, content types, and handlers.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when attempting to set a null value.</exception>
        public HttpApiClientSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value ?? throw new ArgumentNullException(nameof(Settings));
            }
        }

        private readonly bool _disposeHttpClient;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets the base URL for this HTTP API client.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets or sets the default query arguments that will be added to all requests.
        /// </summary>
        public UrlQueryBuilder DefaultArgs { get; set; } = new UrlQueryBuilder();

        /// <summary>
        /// Gets the default request headers that will be sent with all requests.
        /// </summary>
        public HttpRequestHeaders DefaultHeaders => _httpClient.DefaultRequestHeaders;

        /// <summary>
        /// Initializes a new instance of the HttpApiClient class with a new HttpClient and optional base URL.
        /// </summary>
        /// <param name="url">The base URL for API requests. Can be null.</param>
        public HttpApiClient(string url = null) : this(new HttpClient(), true)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the HttpApiClient class with a custom HttpMessageHandler (or DelegatingHandler chain)
        /// and optional base URL. Use this to add interceptors, logging, retry logic, or custom request/response handling.
        /// </summary>
        /// <param name="url">The base URL for API requests. Can be null.</param>
        /// <param name="handler">The HttpMessageHandler to use (e.g., HttpClientHandler, DelegatingHandler). When using DelegatingHandler, ensure InnerHandler is set.</param>
        /// <exception cref="ArgumentNullException">Thrown when handler is null.</exception>
        public HttpApiClient(string url, HttpMessageHandler handler) : this(new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler))), true)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the HttpApiClient class with an existing HttpClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use for requests.</param>
        /// <param name="disposeHttpClient">Whether to dispose the HttpClient when this instance is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when httpClient is null.</exception>
        public HttpApiClient(HttpClient httpClient, bool disposeHttpClient = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _disposeHttpClient = disposeHttpClient;
            Url = httpClient.BaseAddress?.ToString();
        }

        #region Authentication

        /// <summary>
        /// Sets basic authentication using username and password.
        /// </summary>
        /// <param name="username">The username for authentication.</param>
        /// <param name="password">The password for authentication.</param>
        /// <returns>The current HttpApiClient instance for method chaining.</returns>
        public HttpApiClient SetBasicAuthentication(string username, string password)
        {
            SetAuthentication("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            return this;
        }

        /// <summary>
        /// Sets bearer token authentication.
        /// </summary>
        /// <param name="token">The bearer token for authentication.</param>
        /// <returns>The current HttpApiClient instance for method chaining.</returns>
        public HttpApiClient SetBearerAuthentication(string token)
        {
            return SetAuthentication("Bearer", token);
        }

        /// <summary>
        /// Sets API key authentication using a custom header.
        /// </summary>
        /// <param name="apiKey">The API key value.</param>
        /// <param name="headerName">The header name for the API key. Defaults to "X-API-Key".</param>
        /// <returns>The current HttpApiClient instance for method chaining.</returns>
        public HttpApiClient SetApiKeyAuthentication(string apiKey, string headerName = "X-API-Key")
        {
            _httpClient.DefaultRequestHeaders.Remove(headerName);
            _httpClient.DefaultRequestHeaders.Add(headerName, apiKey);
            return this;
        }

        /// <summary>
        /// Sets custom authentication with the specified scheme and value.
        /// </summary>
        /// <param name="scheme">The authentication scheme (e.g., "Basic", "Bearer").</param>
        /// <param name="value">The authentication value.</param>
        /// <returns>The current HttpApiClient instance for method chaining.</returns>
        public HttpApiClient SetAuthentication(string scheme, string value)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
            return this;
        }

        /// <summary>
        /// Clears any existing authentication configuration.
        /// </summary>
        /// <returns>The current HttpApiClient instance for method chaining.</returns>
        public HttpApiClient ClearAuthentication()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return this;
        }

        #endregion

        #region Async Request

        /// <summary>
        /// Sends a GET request with the specified request body and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> GetAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await SendAsync("GET", body, info, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request with the specified request body and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> PostAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await SendAsync("POST", body, info, cancellationToken);
        }

        /// <summary>
        /// Sends a PUT request with the specified request body and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> PutAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await SendAsync("PUT", body, info, cancellationToken);
        }

        /// <summary>
        /// Sends a DELETE request with the specified request body and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> DeleteAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await SendAsync("DELETE", body, info, cancellationToken);
        }

        /// <summary>
        /// Sends a PATCH request with the specified request body and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> PatchAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            return await SendAsync("PATCH", body, info, cancellationToken);
        }

        /// <summary>
        /// Sends an HTTP request with the specified method, request body, and returns a typed response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="httpMethod">The HTTP method to use (GET, POST, PUT, DELETE, PATCH, etc.).</param>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> SendAsync<TResponse>(string httpMethod, IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            if (info == null)
                info = new HttpRequestInfo();

            return await SendAsync(body, info.Clone(httpMethod), cancellationToken);
        }

        /// <summary>
        /// Sends an HTTP request with the specified request body and returns a typed response.
        /// Uses the HTTP method and path configured in the request body's attributes.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="body">The request body object implementing IRequestResponse.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> SendAsync<TResponse>(IRequestResponse<TResponse> body, HttpRequestInfo info = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            using (var response = await SendHttpRequestAsync(body, info, cancellationToken))
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return new RestApiResponse<TResponse>(response, default);

                var deserializer = GetDeserializer<TResponse>(body.GetType(), response);
                if (deserializer != null)
                    return new RestApiResponse<TResponse>(response, await deserializer.DeserializeAsync(response, Settings));

                var content = await response.Content.ReadAsStringAsync();
                var type = HttpApiClientSettings.GetContentType(body.GetType()) ?? Settings.DefaultContentType;
                return new RestApiResponse<TResponse>(response, Settings.Deserialize<TResponse>(content, type));
            }
        }

        /// <summary>
        /// Sends an HTTP request with the specified request information and returns a typed response.
        /// No request body is sent with this method.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response.</typeparam>
        /// <param name="info">The request information including method, path, headers, and query parameters.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the deserialized response data.</returns>
        public async Task<RestApiResponse<TResponse>> SendAsync<TResponse>(HttpRequestInfo info, CancellationToken cancellationToken = default) where TResponse : class
        {
            using (var response = await SendHttpRequestAsync(null, info, cancellationToken))
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return new RestApiResponse<TResponse>(response, default);

                var deserializer = GetDeserializer<TResponse>(typeof(TResponse), response);
                if (deserializer != null)
                    return new RestApiResponse<TResponse>(response, await deserializer.DeserializeAsync(response, Settings));

                var content = await response.Content.ReadAsStringAsync();
                return new RestApiResponse<TResponse>(response, Settings.Deserialize<TResponse>(content));
            }
        }

        /// <summary>
        /// Sends an HTTP request and returns an untyped response.
        /// This method is useful when you don't need to deserialize the response content.
        /// </summary>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A RestApiResponse containing the raw response data.</returns>
        public async Task<RestApiResponse> SendRequestAsync(object body, HttpRequestInfo info = null, CancellationToken cancellationToken = default)
        {
            using (var response = await SendHttpRequestAsync(body, info, cancellationToken))
                return new RestApiResponse(response);
        }

        /// <summary>
        /// Sends an HTTP request and returns the raw HttpResponseMessage.
        /// This method provides the lowest level access to the HTTP response.
        /// </summary>
        /// <param name="body">The request body object. Can be null.</param>
        /// <param name="info">Additional request information. Can be null.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The raw HttpResponseMessage from the request.</returns>
        public async Task<HttpResponseMessage> SendHttpRequestAsync(object body, HttpRequestInfo info = null, CancellationToken cancellationToken = default)
        {
            ValidateBody(body);
            var requestMessage = CreateHttpRequestMessage(ConfigureRequest(info, body), body);
            return await SendRequestAsync(requestMessage, cancellationToken);
        }

        private void ValidateBody(object body)
        {
            if (!Settings.ValidateRequest || body == null || ReflectionUtils.IsNative(body.GetType(), false))
                return;

            var context = new ValidationContext(body, null, null);
            Validator.ValidateObject(body, context, true);
        }
        #endregion

        /// <summary>
        /// Gets the deserializer type for the specified response type by examining the IRequestResponse interface.
        /// </summary>
        /// <param name="responseType">The type to get the deserializer for.</param>
        /// <returns>The deserializer type if found, null otherwise.</returns>
        public static Type GetDeserializerType(Type responseType)
        {
            var interfaceType = responseType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IRequestResponse<,>));

            return interfaceType?.GetGenericArguments()[1];
        }

        private IResponseDeserializer<TResponse> GetDeserializer<TResponse>(Type type, HttpResponseMessage response)
        {
            var converterType = GetDeserializerType(type);
            return converterType == null ? null : Activator.CreateInstance(converterType) as IResponseDeserializer<TResponse>;
        }

        protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await InternalSendRequestAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;

            if (Settings.TryGetHandler<HttpErrorHandler>(response, out var handler))
                throw await handler.MapExceptionAsync(response);

            return response;
        }

        private async Task<HttpResponseMessage> InternalSendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;

            if (Settings.TryGetHandler<IHttpRecoveryHandler>(response, out var handler))
                return await handler.RecoverAsync(new HttpRecoveryContext(_httpClient, request, response, cancellationToken));

            return response;
        }

        internal protected virtual HttpRequestMessage CreateHttpRequestMessage(HttpRequestInfo info, object body)
        {
            if (string.IsNullOrEmpty(Url) && string.IsNullOrEmpty(info.Path))
                throw new InvalidOperationException("Uma URL deve ser definida.");

            var query = info.Query ?? new UrlQueryBuilder();

            if (DefaultArgs != null)
                query = query.Merge(DefaultArgs);

            var request = new HttpRequestMessage(new HttpMethod(info.Method ?? "GET"), query.BuildUrl(Url, info.Path));

            if (info.Headers != null)
                foreach (string name in info.Headers)
                    request.Headers.TryAddWithoutValidation(name, info.Headers[name]);

            if (info.Cookies != null && info.Cookies.Count > 0)
            {
                var cookieHeader = string.Join("; ", info.Cookies.Cast<Cookie>().Select(c => $"{c.Name}={c.Value}"));
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            if (body != null && CanAddBody(info.Method) && Settings.TryParseContent(HttpApiClientSettings.GetContentType(body.GetType()), body, out var content))
                request.Content = content;

            return request;
        }

        private bool CanAddBody(string httpMethod)
        {
            return !string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(httpMethod, "DELETE", StringComparison.OrdinalIgnoreCase);
        }

        private HttpRequestInfo ConfigureRequest(HttpRequestInfo info, object request)
        {
            if (info == null)
                info = new HttpRequestInfo();

            return new HttpRequestInfoBuilder(info)
                .ApplyRequestObject(request)
                .Build();
        }

        /// <summary>
        /// Releases all resources used by the HttpApiClient.
        /// If disposeHttpClient was set to true in the constructor, the underlying HttpClient will also be disposed.
        /// </summary>
        public void Dispose()
        {
            if (_disposeHttpClient)
                _httpClient?.Dispose();
        }
    }
}