namespace JanusRequest
{
    /// <summary>
    /// Specialization of <see cref="IResponseDeserializer{TResponse}"/> for <see cref="ProblemDetails"/>.
    /// Register an implementation via <see cref="HttpApiClientSettings.ProblemDeserializer"/>
    /// to control how error responses are parsed into <see cref="ProblemDetails"/>.
    /// When no implementation is registered, the default JSON deserializer is used.
    /// </summary>
    public interface IProblemDeserializer : IResponseDeserializer<ProblemDetails>
    {
    }
}
