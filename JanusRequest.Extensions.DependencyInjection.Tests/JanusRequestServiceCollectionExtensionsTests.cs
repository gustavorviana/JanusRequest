using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

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
                (provider, httpClient) =>
                {
                    httpClient.BaseAddress = new Uri("https://api.example.com");
                },
                settings =>
                {
                    settings.DefaultContentType = HttpContentType.Xml;
                });

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

            Assert.Equal(HttpContentType.Xml, settings.DefaultContentType);
            Assert.Equal("https://api.example.com/", client.Url);
            Assert.Equal("https://api.example.com/", clientFromFactory.Url);
        }

        [Fact]
        public void AddJanusRequestClient_SupportsNamedClientsThroughFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddJanusRequestClient(
                (provider, httpClient) =>
                {
                    httpClient.BaseAddress = new Uri("https://default.example.com");
                },
                settings => { });

            services.AddJanusRequestClient(
                "users",
                (provider, httpClient) =>
                {
                    httpClient.BaseAddress = new Uri("https://users.example.com");
                });

            var provider = services.BuildServiceProvider();

            // Act
            var factory = provider.GetRequiredService<IHttpApiClientFactory>();
            var defaultClient = factory.CreateClient();
            var namedClient = factory.CreateClient("users");

            // Assert
            Assert.NotNull(factory);
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
                configureClient: null,
                configureSettings: settings =>
                {
                    settings.DefaultContentType = HttpContentType.Xml;
                });

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

            Assert.Equal(HttpContentType.Xml, settings.DefaultContentType);
            Assert.Same(settings, client.Settings);
            Assert.Same(settings, clientFromFactory.Settings);

            Assert.Null(client.Url);
            Assert.Null(clientFromFactory.Url);
        }

        [Fact]
        public void LoggingHttpApiClientLogger_LogsRequestAndResponseAndErrorWithoutHeaders()
        {
            // Arrange
            var settings = new HttpApiClientSettings
            {
                LogResponseHeadersOnError = false
            };
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

            // Assert: main error log
            logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(s => s.ToString().Contains("Error during HTTP request")
                                    && s.ToString().Contains("GET")
                                    && s.ToString().Contains("https://api.example.com/resource")
                                    && s.ToString().Contains("500")),
                exception,
                Arg.Any<Func<object, Exception, string>>());

            // Assert: no extra header log when LogResponseHeadersOnError = false
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
            var settings = new HttpApiClientSettings
            {
                LogResponseHeadersOnError = true
            };
            var logger = Substitute.For<ILogger<HttpApiClient>>();
            var adapter = new LoggingHttpApiClientLogger(logger, settings);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/resource");
            var headers = new Dictionary<string, IReadOnlyList<string>>
            {
                ["X-Test"] = ["a", "b"]
            };
            var exception = new RequestException(HttpStatusCode.BadRequest, "Bad", headers);

            // Act (response can be null when headers come from exception)
            adapter.LogError(exception, request, response: null);

            // Assert: main error log
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
            var settings = new HttpApiClientSettings
            {
                LogResponseHeadersOnError = true
            };
            var logger = Substitute.For<ILogger<HttpApiClient>>();
            var adapter = new LoggingHttpApiClientLogger(logger, settings);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/resource");
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                RequestMessage = request
            };
            response.Headers.Add("X-Header", "v1");
            response.Content = new StringContent("error");
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var exception = new Exception("Failure");

            // Act
            adapter.LogError(exception, request, response);

            // Assert: main error log
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