using JanusRequest.Builders;
using System;
using System.Net.Http.Headers;

namespace JanusRequest
{
    /// <summary>
    /// Base interface for HTTP API clients providing common configuration properties
    /// and authentication methods. Both <see cref="IHttpApiClient"/> and <see cref="IHttpApiDataClient"/>
    /// extend this interface.
    /// </summary>
    public interface IHttpApiClientBase : IDisposable
    {
        /// <summary>
        /// Gets or sets the HTTP API client settings that control serialization, content types, and handlers.
        /// </summary>
        HttpApiClientSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the logger used to record HTTP requests, responses, and errors.
        /// </summary>
        IHttpApiClientLogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the base URL for this HTTP API client.
        /// </summary>
        string Url { get; set; }

        /// <summary>
        /// Gets or sets the default query arguments that will be added to all requests.
        /// </summary>
        UrlQueryBuilder DefaultArgs { get; set; }

        /// <summary>
        /// Gets the default request headers that will be sent with all requests.
        /// </summary>
        HttpRequestHeaders DefaultHeaders { get; }

        /// <summary>
        /// Sets basic authentication using username and password.
        /// </summary>
        IHttpApiClientBase SetBasicAuthentication(string username, string password);

        /// <summary>
        /// Sets bearer token authentication.
        /// </summary>
        IHttpApiClientBase SetBearerAuthentication(string token);

        /// <summary>
        /// Sets API key authentication using a custom header.
        /// </summary>
        IHttpApiClientBase SetApiKeyAuthentication(string apiKey, string headerName = "X-API-Key");

        /// <summary>
        /// Sets custom authentication with the specified scheme and value.
        /// </summary>
        IHttpApiClientBase SetAuthentication(string scheme, string value);

        /// <summary>
        /// Clears any existing authentication configuration.
        /// </summary>
        IHttpApiClientBase ClearAuthentication();
    }
}
