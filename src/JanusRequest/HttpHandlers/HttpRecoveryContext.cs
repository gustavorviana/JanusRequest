using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest.HttpHandlers
{
    /// <summary>
    /// Represents the context for HTTP request recovery operations.
    /// This class encapsulates all the necessary information and functionality needed
    /// to retry or recover from failed HTTP requests, including the original request,
    /// response, HTTP client, and cancellation token.
    /// </summary>
    public class HttpRecoveryContext
    {
        /// <summary>
        /// Gets the HTTP client used for the original request.
        /// </summary>
        public HttpClient Client { get; }

        /// <summary>
        /// Gets the original HTTP request message that needs to be recovered.
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// Gets the HTTP response message from the failed request.
        /// </summary>
        public HttpResponseMessage Response { get; }

        /// <summary>
        /// Gets the cancellation token for controlling the recovery operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Initializes a new instance of the HttpRecoveryContext class.
        /// </summary>
        /// <param name="client">The HTTP client to use for recovery operations.</param>
        /// <param name="request">The original HTTP request that failed.</param>
        /// <param name="response">The HTTP response from the failed request.</param>
        /// <param name="token">The cancellation token for controlling the recovery operation.</param>
        public HttpRecoveryContext(HttpClient client, HttpRequestMessage request, HttpResponseMessage response, CancellationToken token)
        {
            Client = client;
            Request = request;
            Response = response;
            CancellationToken = token;
        }

        /// <summary>
        /// Resends the original HTTP request using the same client and cancellation token.
        /// The request is cloned before sending because HttpRequestMessage is single-use.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous resend operation.
        /// The task result contains the HTTP response from the resent request.
        /// </returns>
        public async Task<HttpResponseMessage> ResendAsync()
        {
            var clone = await CloneRequestAsync(Request);
            return await Client.SendAsync(clone, CancellationToken);
        }

        /// <summary>
        /// Creates a clone of an HTTP request message, including method, URI, headers, version, and content.
        /// This is necessary because HttpRequestMessage cannot be sent more than once.
        /// </summary>
        /// <param name="original">The original request message to clone.</param>
        /// <returns>A new HttpRequestMessage with the same properties as the original.</returns>
        public static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version
            };

            try
            {
                if (original.Content != null)
                {
                    var contentBytes = await original.Content.ReadAsByteArrayAsync();
                    clone.Content = new ByteArrayContent(contentBytes);

                    if (original.Content.Headers != null)
                    {
                        foreach (var header in original.Content.Headers)
                            clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                foreach (var header in original.Headers)
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

#if NET5_0_OR_GREATER
                foreach (var option in original.Options)
                    clone.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
#else
                foreach (var prop in original.Properties)
                    clone.Properties[prop.Key] = prop.Value;
#endif

                return clone;
            }
            catch
            {
                clone.Dispose();
                throw;
            }
        }
    }
}