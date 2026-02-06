namespace JanusRequest.Extensions.DependencyInjection
{
    /// <summary>
    /// Default implementation of <see cref="IHttpApiClientFactory"/> that is backed by
    /// <see cref="IHttpClientFactory"/> and a shared <see cref="HttpApiClientSettings"/> instance.
    /// </summary>
    internal sealed class HttpApiClientFactory : IHttpApiClientFactory
    {
        internal const string DefaultClientName = "JanusRequest.Default";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpApiClientSettings _settings;
        private readonly IHttpApiClientLogger _logger;

        public HttpApiClientFactory(IHttpClientFactory httpClientFactory, HttpApiClientSettings settings, IHttpApiClientLogger logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public HttpApiClient CreateClient()
        {
            return CreateClient(DefaultClientName);
        }

        public HttpApiClient CreateClient(string name)
        {
            var effectiveName = string.IsNullOrWhiteSpace(name) ? DefaultClientName : name;
            var httpClient = _httpClientFactory.CreateClient(effectiveName);
            return new HttpApiClient(httpClient)
            {
                Settings = _settings,
                Logger = _logger
            };
        }
    }
}

