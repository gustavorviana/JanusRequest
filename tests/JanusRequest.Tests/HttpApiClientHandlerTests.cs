using JanusRequest.HttpHandlers;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientHandlerTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendAsync_WithErrorHandler_StoresHandlerExceptionOnResponseAsync()
        {
            var request = new TestRequest();
            var errorHandler = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("API Error");

            errorHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            errorHandler.MapExceptionAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(errorHandler);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            var response = await _httpApiClient.SendAsync(request);

            var ex = Assert.Throws<Exception>(() => response.EnsureSuccessStatusCode());
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithRecoveryHandler_RecoversRequestAsync()
        {
            var request = new TestRequest();
            var recoveryHandler = Substitute.For<IHttpRecoveryHandler>();
            var recoveredResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"Id\":1,\"Name\":\"Recovered\"}")
            };

            recoveryHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            recoveryHandler.RecoverAsync(Arg.Any<HttpRecoveryContext>()).Returns(Task.FromResult(recoveredResponse));

            _settings.SetHandlers(recoveryHandler);
            SetupHttpResponse(HttpStatusCode.InternalServerError, "Error");

            var result = await _httpApiClient.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal("Recovered", result.Data.Name);
        }

        [Fact]
        public async Task SendAsync_WithMultipleHandlers_UsesCorrectHandlerAsync()
        {
            var request = new TestRequest();
            var handler1 = Substitute.For<IHttpHandlerBase>();
            var handler2 = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("Handler 2 Error");

            handler1.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            handler2.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            handler2.MapExceptionAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(handler1, handler2);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            var response = await _httpApiClient.SendAsync(request);
            var ex = Assert.Throws<Exception>(() => response.EnsureSuccessStatusCode());
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithNoMatchingHandler_FallsBackToDefaultOnEnsureSuccessAsync()
        {
            var request = new TestRequest();
            var handler = Substitute.For<IHttpErrorHandler>();

            handler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(handler);
            SetupHttpResponse(HttpStatusCode.BadRequest, null!);

            var response = await _httpApiClient.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode);
            var ex = Assert.Throws<RequestException>(() => response.EnsureSuccessStatusCode());
            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            await handler.DidNotReceive().MapExceptionAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>());
        }

        [Fact]
        public async Task SendAsync_WithNoHandlersRegistered_FallsBackToDefaultOnEnsureSuccessAsync()
        {
            var request = new TestRequest();
            SetupHttpResponse(HttpStatusCode.NotFound, "not found");

            var response = await _httpApiClient.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode);
            var ex = Assert.Throws<RequestException>(() => response.EnsureSuccessStatusCode());
            Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WithRecoveryHandlerThatCannotHandle_DoesNotRecoverAsync()
        {
            var request = new TestRequest();
            var recoveryHandler = Substitute.For<IHttpRecoveryHandler>();

            recoveryHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(recoveryHandler);
            SetupHttpResponse(HttpStatusCode.InternalServerError, null!);

            var response = await _httpApiClient.SendAsync(request);

            Assert.False(response.IsSuccessStatusCode);
            var ex = Assert.Throws<RequestException>(() => response.EnsureSuccessStatusCode());
            Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
            await recoveryHandler.DidNotReceive().RecoverAsync(Arg.Any<HttpRecoveryContext>());
        }
    }
}
