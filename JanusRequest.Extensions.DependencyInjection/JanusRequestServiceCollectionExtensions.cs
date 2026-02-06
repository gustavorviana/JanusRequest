using System;
using System.Net.Http;
using JanusRequest;
using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="configureClient">
        /// Optional callback to configure the underlying HttpClient (BaseAddress, default headers, etc.).
        /// </param>
        /// <param name="configureSettings">
        /// Optional callback to customize HttpApiClientSettings for this application.
        /// If not provided, HttpApiClientSettings.Default is used.
        /// </param>
        /// <returns>The IHttpClientBuilder for further configuration.</returns>
        public static IHttpClientBuilder AddJanusRequestClient(
            this IServiceCollection services,
            Action<IServiceProvider, HttpClient>? configureClient = null,
            Action<HttpApiClientSettings>? configureSettings = null)
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

            // Logger adapter for IHttpApiClientLogger -> ILogger<HttpApiClient>
            services.AddSingleton<IHttpApiClientLogger, LoggingHttpApiClientLogger>();

            // Register our factory abstraction so callers can request IHttpApiClientFactory
            services.AddTransient<IHttpApiClientFactory, HttpApiClientFactory>();

            // Register a named HttpClient that both the factory and typed client will use
            var builder = services
                .AddHttpClient(HttpApiClientFactory.DefaultClientName, (provider, httpClient) =>
                {
                    configureClient?.Invoke(provider, httpClient);
                });

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
            Action<IServiceProvider, HttpClient>? configureClient = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Client name must be provided.", nameof(name));

            return services
                .AddHttpClient(name, (service, httpClient) => configureClient?.Invoke(service, httpClient))
                .AddTypedClient((httpClient, provider) =>
                {
                    var settings = provider.GetService<HttpApiClientSettings>() ?? HttpApiClientSettings.Default;
                    return new HttpApiClient(httpClient)
                    {
                        Settings = settings
                    };
                });
        }
    }
}