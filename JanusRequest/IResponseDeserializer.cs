using System.Net.Http;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Interface for custom response deserializers that handle specific response processing logic.
    /// Implementations of this interface provide custom deserialization behavior for HTTP responses,
    /// allowing for specialized handling of response content based on specific requirements or formats.
    /// </summary>
    /// <typeparam name="TResponse">The type of object to deserialize the HTTP response to.</typeparam>
    public interface IResponseDeserializer<TResponse>
    {
        /// <summary>
        /// Asynchronously deserializes an HTTP response message to the specified response type.
        /// This method provides access to the complete HTTP response including headers, status code,
        /// and content, allowing for custom deserialization logic that may depend on response metadata.
        /// </summary>
        /// <param name="response">The HTTP response message to deserialize.</param>
        /// <param name="settings">The HTTP API client settings containing configuration for deserialization.</param>
        /// <returns>
        /// A task that represents the asynchronous deserialization operation.
        /// The task result contains the deserialized response object of type TResponse.
        /// </returns>
        Task<TResponse> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings);
    }
}