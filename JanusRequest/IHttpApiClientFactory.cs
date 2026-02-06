namespace JanusRequest
{
    /// <summary>
    /// Factory abstraction for creating <see cref="HttpApiClient"/> instances.
    /// Inspired by <c>IHttpClientFactory</c>, it allows consumers to obtain
    /// configured <see cref="HttpApiClient"/> objects without coupling to
    /// a specific construction mechanism (DI container, manual wiring, etc.).
    /// </summary>
    public interface IHttpApiClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="HttpApiClient"/> instance using a default configuration.
        /// </summary>
        /// <returns>A new <see cref="HttpApiClient"/> instance.</returns>
        HttpApiClient CreateClient();

        /// <summary>
        /// Creates a new <see cref="HttpApiClient"/> instance using a named configuration.
        /// The meaning of the <paramref name="name"/> parameter is implementation-specific,
        /// but typically maps to a logical client configuration (base URL, headers, etc.).
        /// </summary>
        /// <param name="name">The logical name of the client configuration.</param>
        /// <returns>A new <see cref="HttpApiClient"/> instance.</returns>
        HttpApiClient CreateClient(string name);
    }
}

