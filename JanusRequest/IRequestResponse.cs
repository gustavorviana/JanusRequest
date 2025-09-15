namespace JanusRequest
{
    /// <summary>
    /// Interface for request objects that expect a specific response type and use a custom deserializer.
    /// This interface extends IRequestResponse&lt;TResponse&gt; to specify both the expected response type
    /// and the deserializer that should be used to process the HTTP response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response object.</typeparam>
    /// <typeparam name="TDeserializer">The type of the custom deserializer that implements IResponseDeserializer&lt;TResponse&gt;.</typeparam>
    public interface IRequestResponse<TResponse, TDeserializer>
        : IRequestResponse<TResponse> where TResponse : class
        where TDeserializer : IResponseDeserializer<TResponse>
    {
    }

    /// <summary>
    /// Base interface for request objects that expect a specific response type.
    /// This interface is used to mark request objects and associate them with their expected response type,
    /// enabling the HttpApiClient to automatically deserialize responses to the correct type.
    /// </summary>
    /// <typeparam name="TResponse">The type of the expected response object.</typeparam>
    public interface IRequestResponse<TResponse> where TResponse : class
    {
    }
}