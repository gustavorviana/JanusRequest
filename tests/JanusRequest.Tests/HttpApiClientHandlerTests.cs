using JanusRequest.HttpHandlers;
using NSubstitute;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientHandlerTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task SendAsync_WithErrorHandler_ThrowsExceptionAsync()
        {
            // Arrange
            var request = new TestRequest();
            var errorHandler = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("API Error");

            errorHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            errorHandler.MapExceptionAsync(Arg.Any<HttpResponseMessage>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(errorHandler);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _httpApiClient.SendAsync(request));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithRecoveryHandler_RecoversRequestAsync()
        {
            // Arrange
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

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
            Assert.Equal("Recovered", result.Data.Name);
        }

        [Fact]
        public async Task SendAsync_WithMultipleHandlers_UsesCorrectHandlerAsync()
        {
            // Arrange
            var request = new TestRequest();
            var handler1 = Substitute.For<IHttpHandlerBase>();
            var handler2 = Substitute.For<HttpErrorHandler>();
            var expectedException = new Exception("Handler 2 Error");

            handler1.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            handler2.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(true);
            handler2.MapExceptionAsync(Arg.Any<HttpResponseMessage>()).Returns(Task.FromResult(expectedException));

            _settings.SetHandlers(handler1, handler2);
            SetupHttpResponse(HttpStatusCode.BadRequest, "Error");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _httpApiClient.SendAsync(request));
            Assert.Same(expectedException, ex);
        }

        [Fact]
        public async Task SendAsync_WithNoMatchingHandler_ReturnsOriginalResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            var handler = Substitute.For<HttpErrorHandler>();

            handler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(handler);
            SetupHttpResponse(HttpStatusCode.BadRequest, null!);

            // Act
            await Assert.ThrowsAsync<DeserializationException>(async () => await _httpApiClient.SendAsync(request));

            // Assert
            await handler.DidNotReceive().MapExceptionAsync(Arg.Any<HttpResponseMessage>());
        }

        [Fact]
        public async Task InternalSendRequestAsync_WithRecoveryHandlerThatCannotHandle_ReturnsOriginalResponseAsync()
        {
            // Arrange
            var request = new TestRequest();
            var recoveryHandler = Substitute.For<IHttpRecoveryHandler>();

            recoveryHandler.CanHandle(Arg.Any<HttpResponseMessage>()).Returns(false);
            _settings.SetHandlers(recoveryHandler);
            SetupHttpResponse(HttpStatusCode.InternalServerError, null!);

            // Act
            var result = await Assert.ThrowsAsync<DeserializationException>(async () => await _httpApiClient.SendAsync(request));

            // Assert
            await recoveryHandler.DidNotReceive().RecoverAsync(Arg.Any<HttpRecoveryContext>());
        }
    }
}
