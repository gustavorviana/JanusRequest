using System;
using System.Net.Http;

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
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpApiClientSettings _settings;
        private readonly IHttpApiClientLogger _logger;
        private readonly HttpApiClientConfiguratorRegistry _configuratorRegistry;

        public HttpApiClientFactory(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, HttpApiClientSettings settings, IHttpApiClientLogger logger, HttpApiClientConfiguratorRegistry configuratorRegistry = null)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuratorRegistry = configuratorRegistry;
        }

        public HttpApiClient CreateClient()
        {
            return CreateClient(DefaultClientName);
        }

        public HttpApiClient CreateClient(string name)
        {
            var effectiveName = string.IsNullOrWhiteSpace(name) ? DefaultClientName : name;
            var httpClient = _httpClientFactory.CreateClient(effectiveName);

            var httpApiClient = new HttpApiClient(httpClient)
            {
                Settings = _settings,
                Logger = _logger
            };

            _configuratorRegistry?.Get(effectiveName)?.Invoke(_serviceProvider, httpApiClient);

            return httpApiClient;
        }
    }
}