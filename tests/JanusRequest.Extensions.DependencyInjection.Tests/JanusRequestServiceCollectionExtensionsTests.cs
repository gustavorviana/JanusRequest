using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

#pragma warning disable CS8602
#pragma warning disable CS8620

namespace JanusRequest.Extensions.DependencyInjection.Tests
{
    public class JanusRequestServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddJanusRequestClient_RegistersHttpApiClientSettingsAndFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(
                settings => settings.DefaultMediaType = HttpContentType.Xml)
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://api.example.com"));

            var provider = services.BuildServiceProvider();

            // Act
            var client = provider.GetRequiredService<HttpApiClient>();
            var settings = provider.GetRequiredService<HttpApiClientSettings>();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();
            var clientFromFactory = factory.CreateClient();

            // Assert
            Assert.NotNull(client);
            Assert.NotNull(settings);
            Assert.NotNull(factory);
            Assert.NotNull(clientFromFactory);

            Assert.Same(settings, client.Settings);
            Assert.Same(settings, clientFromFactory.Settings);

            Assert.Equal(HttpContentType.Xml, settings.DefaultMediaType);
            Assert.Equal("https://api.example.com/", client.Url);
            Assert.Equal("https://api.example.com/", clientFromFactory.Url);
        }

        [Fact]
        public void AddJanusRequestClient_SupportsNamedClientsThroughFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(settings => { })
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://default.example.com"));

            services.AddHttpClient("users", (provider, httpClient) =>
                httpClient.BaseAddress = new Uri("https://users.example.com"));

            var provider = services.BuildServiceProvider();

            // Act
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();
            var defaultClient = factory.CreateClient();
            var namedClient = factory.CreateClient("users");

            // Assert
            Assert.NotNull(defaultClient);
            Assert.NotNull(namedClient);
            Assert.Equal("https://default.example.com/", defaultClient.Url);
            Assert.Equal("https://users.example.com/", namedClient.Url);
        }

        [Fact]
        public void AddJanusRequestClient_WithAllDefaults_UsesHttpApiClientSettingsDefault()
        {
            // Arrange
            var originalDefault = HttpApiClientSettings.Default;
            var services = new ServiceCollection();

            services.AddJanusRequestClient();

            var provider = services.BuildServiceProvider();

            // Act
            var client = provider.GetRequiredService<HttpApiClient>();
            var settings = provider.GetRequiredService<HttpApiClientSettings>();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();
            var clientFromFactory = factory.CreateClient();

            // Assert
            Assert.NotNull(client);
            Assert.NotNull(settings);
            Assert.NotNull(factory);
            Assert.NotNull(clientFromFactory);

            Assert.Same(HttpApiClientSettings.Default, settings);
            Assert.Same(settings, client.Settings);
            Assert.Same(settings, clientFromFactory.Settings);

            Assert.Null(client.Url);
            Assert.Null(clientFromFactory.Url);
            Assert.Same(originalDefault, HttpApiClientSettings.Default);
        }

        [Fact]
        public void AddJanusRequestClient_WithOnlySettingsConfigured_DoesNotSetBaseAddress()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(
                configureSettings: settings => settings.DefaultMediaType = HttpContentType.Xml);

            var provider = services.BuildServiceProvider();

            // Act
            var client = provider.GetRequiredService<HttpApiClient>();
            var settings = provider.GetRequiredService<HttpApiClientSettings>();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();
            var clientFromFactory = factory.CreateClient();

            // Assert
            Assert.Equal(HttpContentType.Xml, settings.DefaultMediaType);
            Assert.Same(settings, client.Settings);
            Assert.Same(settings, clientFromFactory.Settings);
            Assert.Null(client.Url);
            Assert.Null(clientFromFactory.Url);
        }

        [Fact]
        public void AddJanusRequestClient_RegistersIHttpApiClient_ResolvesToHttpApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJanusRequestClient();
            var provider = services.BuildServiceProvider();

            // Act
            var iHttpApiClient = provider.GetRequiredService<IHttpApiClient>();

            // Assert
            Assert.NotNull(iHttpApiClient);
            Assert.IsType<HttpApiClient>(iHttpApiClient);
        }

        [Fact]
        public void AddJanusRequestClient_RegistersIHttpApiDataClient_ResolvesToHttpApiClient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddJanusRequestClient();
            var provider = services.BuildServiceProvider();

            // Act
            var dataClient = provider.GetRequiredService<IHttpApiDataClient>();

            // Assert
            Assert.NotNull(dataClient);
            Assert.IsType<HttpApiClient>(dataClient);
        }

        // Named client overload tests

        [Fact]
        public void AddJanusRequestClient_Named_FactoryResolvesCorrectBaseAddress()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(settings => { })
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://default.example.com"));

            services.AddJanusRequestClient("payments")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://payments.example.com"));

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act
            var paymentsClient = factory.CreateClient("payments");

            // Assert
            Assert.Equal("https://payments.example.com/", paymentsClient.Url);
        }

        [Fact]
        public void AddJanusRequestClient_Named_ConfigureActionIsAppliedToClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureApplied = false;

            services.AddJanusRequestClient(settings => { });

            services.AddJanusRequestClient("payments", (provider, client) =>
            {
                configureApplied = true;
                client.SetBearerAuthentication("test-token");
            }).ConfigureHttpClient((provider, httpClient) =>
                httpClient.BaseAddress = new Uri("https://payments.example.com"));

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act
            var client = factory.CreateClient("payments");

            // Assert
            Assert.True(configureApplied);
            Assert.Equal("Bearer", client.DefaultHeaders.Authorization!.Scheme);
            Assert.Equal("test-token", client.DefaultHeaders.Authorization.Parameter);
        }

        [Fact]
        public void AddJanusRequestClient_Named_MultipleClientsAreIndependent()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(settings => { });

            services.AddJanusRequestClient("payments")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://payments.example.com"));

            services.AddJanusRequestClient("notifications")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://notifications.example.com"));

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act
            var paymentsClient = factory.CreateClient("payments");
            var notificationsClient = factory.CreateClient("notifications");

            // Assert
            Assert.Equal("https://payments.example.com/", paymentsClient.Url);
            Assert.Equal("https://notifications.example.com/", notificationsClient.Url);
            Assert.NotEqual(paymentsClient.Url, notificationsClient.Url);
        }

        [Fact]
        public void AddJanusRequestClient_Named_WithoutConfigureAction_ReturnsClientWithDefaultSettings()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(settings => { });

            services.AddJanusRequestClient("bare")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://bare.example.com"));

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act
            var client = factory.CreateClient("bare");
            var settings = provider.GetRequiredService<HttpApiClientSettings>();

            // Assert
            Assert.Equal("https://bare.example.com/", client.Url);
            Assert.Same(settings, client.Settings);
            Assert.Null(client.DefaultHeaders.Authorization);
        }

        [Fact]
        public void AddJanusRequestClient_Named_NullName_ThrowsArgumentException()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() =>
                services.AddJanusRequestClient(null, configureClient: null));
        }

        [Fact]
        public void AddJanusRequestClient_Named_EmptyName_ThrowsArgumentException()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() =>
                services.AddJanusRequestClient("   ", configureClient: null));
        }

        [Fact]
        public void AddJanusRequestClient_Named_TryRegisterCore_CalledMultipleTimes_DoesNotDuplicateRegistrations()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(settings => { });

            services.AddJanusRequestClient("payments")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://payments.example.com"));

            services.AddJanusRequestClient("notifications")
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://notifications.example.com"));

            // Act - TryAddSingleton must ensure a single registration per type
            var factoryRegistrations = services.Count(x => x.ServiceType == typeof(IHttpApiClientFactory));
            var loggerRegistrations = services.Count(x => x.ServiceType == typeof(IHttpApiClientLogger));

            // Assert
            Assert.Equal(1, factoryRegistrations);
            Assert.Equal(1, loggerRegistrations);
        }

        [Fact]
        public void CreateClient_WithNullName_AppliesDefaultConfigurator()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuratorApplied = false;

            services.AddJanusRequestClient(settings => { })
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://default.example.com"));

            // Register a named client using the default client name via reflection
            services.AddJanusRequestClient("JanusRequest.Default", (provider, client) =>
            {
                configuratorApplied = true;
            });

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act - pass null name, which should resolve to DefaultClientName
            var client = factory.CreateClient(null!);

            // Assert
            Assert.True(configuratorApplied, "Configurator should have been applied for the default client name");
        }

        [Fact]
        public void CreateClient_WithEmptyName_AppliesDefaultConfigurator()
        {
            // Arrange
            var services = new ServiceCollection();
            var configuratorApplied = false;

            services.AddJanusRequestClient(settings => { })
                .ConfigureHttpClient((provider, httpClient) =>
                    httpClient.BaseAddress = new Uri("https://default.example.com"));

            services.AddJanusRequestClient("JanusRequest.Default", (provider, client) =>
            {
                configuratorApplied = true;
            });

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();

            // Act - pass empty name, which should resolve to DefaultClientName
            var client = factory.CreateClient("");

            // Assert
            Assert.True(configuratorApplied, "Configurator should have been applied for the default client name");
        }

        // Logging tests

        [Fact]
        public void LoggingHttpApiClientLogger_LogsRequestAndResponseAndErrorWithoutHeaders()
        {
            // Arrange
            var settings = new HttpApiClientSettings { LogResponseHeadersOnError = false };
            var logger = Substitute.For<ILogger<HttpApiClient>>();
            var adapter = new LoggingHttpApiClientLogger(logger, settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/resource");
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                RequestMessage = request
            };
            var exception = new Exception("Failure");

            // Act
            adapter.LogRequest(request);
            adapter.LogResponse(response);
            adapter.LogError(exception, request, response);

            // Assert: request log
            logger.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString() == "Sending HTTP GET https://api.example.com/resource"),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());

            // Assert: response log
            logger.Received(1).Log(
                LogLevel.Debug,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString() == "Received HTTP 500 from https://api.example.com/resource"),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());

            // Assert: error log
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("Error during HTTP request")
                                    && s.ToString().Contains("GET")
                                    && s.ToString().Contains("https://api.example.com/resource")
                                    && s.ToString().Contains("500")),
                exception,
                Arg.Any<Func<object, Exception, string>>());

            // Assert: no header log when LogResponseHeadersOnError is false
            logger.DidNotReceive().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().StartsWith("HTTP response headers for")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LoggingHttpApiClientLogger_LogsHeadersFromRequestExceptionWhenEnabled()
        {
            // Arrange
            var settings = new HttpApiClientSettings { LogResponseHeadersOnError = true };
            var logger = Substitute.For<ILogger<HttpApiClient>>();
            var adapter = new LoggingHttpApiClientLogger(logger, settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/resource");
            var headers = new Dictionary<string, IReadOnlyList<string>>
            {
                ["X-Test"] = ["a", "b"]
            };
            var exception = new RequestException(HttpStatusCode.BadRequest, "Bad", headers);

            // Act
            adapter.LogError(exception, request, response: null);

            // Assert: error log
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("Error during HTTP request")),
                exception,
                Arg.Any<Func<object, Exception, string>>());

            // Assert: header log built from RequestException.Headers
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("HTTP response headers for")
                                    && s.ToString().Contains("X-Test=a,b")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public void LoggingHttpApiClientLogger_LogsHeadersFromResponseWhenEnabledAndNoRequestException()
        {
            // Arrange
            var settings = new HttpApiClientSettings { LogResponseHeadersOnError = true };
            var logger = Substitute.For<ILogger<HttpApiClient>>();
            var adapter = new LoggingHttpApiClientLogger(logger, settings);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/resource");
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = request,
                Content = new StringContent("error")
            };
            response.Headers.Add("X-Header", "v1");
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var exception = new Exception("Failure");

            // Act
            adapter.LogError(exception, request, response);

            // Assert: error log
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("Error during HTTP request")),
                exception,
                Arg.Any<Func<object, Exception, string>>());

            // Assert: header log built from HttpResponseMessage headers
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("HTTP response headers for")
                                    && s.ToString().Contains("X-Header=v1")
                                    && s.ToString().Contains("Content-Type=application/json")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception, string>>());
        }
    }
}