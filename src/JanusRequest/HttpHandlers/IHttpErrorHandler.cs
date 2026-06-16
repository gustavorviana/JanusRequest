using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Handler responsible for mapping unsuccessful HTTP responses to exceptions.
    /// Implementations decide which responses they apply to via <see cref="IHttpHandlerBase.CanHandle"/>
    /// and produce the exception that should be thrown by the client.
    /// </summary>
    public interface IHttpErrorHandler : IHttpHandlerBase
    {
        /// <summary>
        /// Maps an unsuccessful HTTP response to the exception that will be thrown.
        /// </summary>
        /// <param name="response">The HTTP response to map.</param>
        /// <param name="settings">The settings of the client that produced the response, used to access shared services such as the problem details deserializer.</param>
        /// <param name="cancellationToken">Cancellation token propagated from the originating request.</param>
        Task<Exception> MapExceptionAsync(HttpResponseMessage response, HttpApiClientSettings settings, CancellationToken cancellationToken = default);
    }
}
