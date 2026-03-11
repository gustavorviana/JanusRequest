using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Net.Http;

namespace JanusRequest.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for integrating HttpApiClient with ASP.NET Core DI and IHttpClientFactory.
    /// </summary>
    public static class JanusRequestServiceCollectionExtensions
    {
        /// <summary>
        /// Registers HttpApiClient as a typed client using IHttpClientFactory.
        /// This overload uses a shared HttpApiClientSettings instance that can be customized.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureSettings">
        /// Optional callback to customize HttpApiClientSettings for this application.
        /// If not provided, HttpApiClientSettings.Default is used.
        /// </param>
        /// <returns>The IHttpClientBuilder for further configuration.</returns>
        public static IHttpClientBuilder AddJanusRequestClient(
            this IServiceCollection services,
            Action<HttpApiClientSettings> configureSettings = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(provider =>
            {
                if (configureSettings == null)
                    return HttpApiClientSettings.Default;

                var settings = new HttpApiClientSettings();
                configureSettings(settings);
                return settings;
            });

            TryRegisterCore(services);

            // Register a named HttpClient that both the factory and typed client will use
            var builder = services.AddHttpClient(HttpApiClientFactory.DefaultClientName);

            // Also register HttpApiClient as a typed client for those who prefer direct injection
            builder.AddTypedClient((httpClient, provider) =>
            {
                var settings = provider.GetRequiredService<HttpApiClientSettings>();
                var logger = provider.GetRequiredService<IHttpApiClientLogger>();
                return new HttpApiClient(httpClient)
                {
                    Settings = settings,
                    Logger = logger
                };
            });

            return builder;
        }

        /// <summary>
        /// Registers a named HttpApiClient using IHttpClientFactory.
        /// Use this overload when you need multiple differently configured JanusRequest clients.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="name">The logical name for the HttpClient.</param>
        /// <param name="configureClient">
        /// Optional callback to configure the underlying HttpClient for this named client.
        /// </param>
        /// <returns>The IHttpClientBuilder for further configuration.</returns>
        public static IHttpClientBuilder AddJanusRequestClient(
            this IServiceCollection services,
            string name,
            Action<IServiceProvider, HttpApiClient> configureClient = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Client name must be provided.", nameof(name));

            TryRegisterCore(services);

            var registry = services.FirstOrDefault(x => x.ImplementationType == typeof(HttpApiClientConfiguratorRegistry))?.ImplementationInstance as HttpApiClientConfiguratorRegistry;
            if (registry == null)
            {
                registry = new HttpApiClientConfiguratorRegistry();
                services.AddSingleton(registry);
            }

            registry.Register(name, configureClient);

            return services.AddHttpClient(name);
        }

        private static void TryRegisterCore(IServiceCollection services)
        {
            // Logger adapter for IHttpApiClientLogger -> ILogger<HttpApiClient>
            services.TryAddSingleton<IHttpApiClientLogger, LoggingHttpApiClientLogger>();

            // Register our factory abstraction so callers can request IHttpApiClientFactory
            services.TryAddSingleton<IHttpApiClientFactory>(x =>
            {
                var clientFactory = x.GetRequiredService<IHttpClientFactory>();
                var settings = x.GetRequiredService<HttpApiClientSettings>();
                var logger = x.GetRequiredService<IHttpApiClientLogger>();

                return new HttpApiClientFactory(x, clientFactory, settings, logger)
                {
                    ConfiguratorRegistry = x.GetService<HttpApiClientConfiguratorRegistry>()
                };
            });
        }
    }
}