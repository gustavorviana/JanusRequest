using JanusRequest.Attributes;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JanusRequest.Tests
{
    public abstract class HttpApiClientTestBase : IDisposable
    {
        protected readonly HttpClient _httpClient;
        protected readonly HttpApiClient _httpApiClient;
        protected readonly HttpApiClientSettings _settings;
        protected readonly MockHttpMessageHandler _httpMessageHandler;

        protected HttpApiClientTestBase()
        {
            _httpMessageHandler = Substitute.For<MockHttpMessageHandler>();
            _settings = new HttpApiClientSettings();

            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            _httpApiClient = new HttpApiClient(_httpClient, true)
            {
                Settings = _settings
            };
        }

        public void Dispose()
        {
            _httpApiClient?.Dispose();
        }

        protected void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode);

            if (content != null)
                response.Content = new StringContent(content);

            _httpMessageHandler.OnSendedAsync(
                Arg.Any<HttpRequestMessage>(),
                Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(response));
        }

        public class TestResponse
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        public class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage? Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            };

            protected sealed override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return OnSendedAsync(request, cancellationToken);
            }

            public virtual Task<HttpResponseMessage> OnSendedAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response!);
            }
        }

        [Request("http://localhost/test")]
        public class TestRequest : IRequestResponse<TestResponse>
        {
        }

        [Request("http://localhost/test")]
        public class TestArrayRequest : IRequestResponse<TestResponse[]>
        {
        }

        [Request("http://localhost/test")]
        public class TestIListRequest : IRequestResponse<IList<TestResponse>>
        {
        }

        [Request("http://localhost/test")]
        public class TestICollectionRequest : IRequestResponse<ICollection<TestResponse>>
        {
        }

        [Request("http://localhost/test", Method = "POST")]
        public class ValidatedRequest : IRequestResponse<TestResponse>
        {
            [Required(ErrorMessage = "Name is required")]
            public string? Name { get; set; }
        }

        [ContentType(HttpContentType.QueryString)]
        public class UnsupportedContentTypeRequest : IRequestResponse<TestResponse>
        {
        }

        protected class TestLogger : IHttpApiClientLogger
        {
            public int RequestCount { get; private set; }
            public int ResponseCount { get; private set; }
            public int ErrorCount { get; private set; }

            public HttpRequestMessage? LastRequest { get; private set; }
            public HttpResponseMessage? LastResponse { get; private set; }
            public Exception? LastException { get; private set; }

            public void LogRequest(HttpRequestMessage request)
            {
                RequestCount++;
                LastRequest = request;
            }

            public void LogError(Exception exception, HttpRequestMessage request, HttpResponseMessage response)
            {
                ErrorCount++;
                LastException = exception;
                LastRequest = request;
                LastResponse = response;
            }

            public void LogResponse(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
            {
                ResponseCount++;
                LastResponse = response;
            }
        }
    }
}
