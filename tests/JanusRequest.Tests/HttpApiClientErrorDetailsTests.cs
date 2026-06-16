using NSubstitute;
using System.Net;
using System.Net.Http;

namespace JanusRequest.Tests
{
    public class HttpApiClientErrorDetailsTests : HttpApiClientTestBase
    {
        private static readonly string ProblemDetailsJson = @"{
            ""type"": ""https://example.com/validation"",
            ""title"": ""Validation Error"",
            ""status"": 422,
            ""detail"": ""One or more fields are invalid."",
            ""instance"": ""/users/register"",
            ""traceId"": ""abc-123""
        }";

        private static ProblemDetailsException AssertProblemDetailsException(RestApiResponse response)
        {
            var ex = Assert.Throws<ProblemDetailsException>(() => response.EnsureSuccessStatusCode());
            return ex;
        }

        private static T AssertExceptionFromResponse<T>(RestApiResponse response) where T : System.Exception
        {
            return Assert.Throws<T>(() => response.EnsureSuccessStatusCode());
        }

        #region Body capture

        [Fact]
        public async Task SendAsync_SuccessResponse_DoesNotThrowOnEnsureSuccess()
        {
            SetupHttpResponse(HttpStatusCode.OK, @"{ ""id"": 1, ""name"": ""test"" }");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            result.EnsureSuccessStatusCode();
            Assert.True(result.IsSuccessStatusCode);
        }

        [Fact]
        public async Task SendAsync_ErrorResponse_BodyIsCarriedOnRequestException()
        {
            SetupHttpResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

            var result = await _httpApiClient.SendRequestAsync(null,
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertExceptionFromResponse<RequestException>(result);
            Assert.Equal("Internal Server Error", ex.Response);
        }

        #endregion

        #region ProblemDetails

        [Fact]
        public async Task SendAsync_ErrorWithProblemDetails_ThrowsProblemDetailsExceptionWithParsedFields()
        {
            SetupHttpResponse(HttpStatusCode.UnprocessableEntity, ProblemDetailsJson);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertProblemDetailsException(result);
            Assert.NotNull(ex.Problem);
            Assert.Equal("https://example.com/validation", ex.Problem.Type);
            Assert.Equal("Validation Error", ex.Problem.Title);
            Assert.Equal(422, ex.Problem.Status);
            Assert.Equal("One or more fields are invalid.", ex.Problem.Detail);
            Assert.Equal("/users/register", ex.Problem.Instance);
            Assert.Equal("abc-123", ex.Problem.Extensions["traceId"].Value);
        }

        [Fact]
        public async Task SendAsync_ErrorWithNonProblemDetails_ThrowsRequestExceptionNotProblemDetails()
        {
            SetupHttpResponse(HttpStatusCode.InternalServerError, "plain text error");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertExceptionFromResponse<RequestException>(result);
            Assert.IsNotType<ProblemDetailsException>(ex);
        }

        [Fact]
        public async Task SendAsync_ErrorWithProblemDetailsExtensions_ParsesExtensions()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""errors"": [
                    { ""field"": ""name"", ""message"": ""required"" },
                    { ""field"": ""email"", ""message"": ""invalid"" }
                ]
            }";
            SetupHttpResponse(HttpStatusCode.BadRequest, json);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertProblemDetailsException(result);
            var errors = ex.Problem.Extensions["errors"];
            Assert.True(errors.HasChildren);
            Assert.Equal("name", errors.Children["0"].Children["field"].Value);
            Assert.Equal("required", errors.Children["0"].Children["message"].Value);
        }

        [Fact]
        public async Task SendRequestAsync_ErrorWithProblemDetails_ThrowsProblemDetailsException()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest, ProblemDetailsJson);

            var result = await _httpApiClient.SendRequestAsync(null,
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertProblemDetailsException(result);
            Assert.Equal("Validation Error", ex.Problem.Title);
        }

        [Fact]
        public async Task SendAsync_NoContent_DoesNotThrow()
        {
            SetupHttpResponse(HttpStatusCode.NoContent, null);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "DELETE", Path = "/test" });

            result.EnsureSuccessStatusCode();
        }

        #endregion

        #region Defaults

        [Fact]
        public void ProblemDeserializer_DefaultIsNull()
        {
            var settings = new HttpApiClientSettings();
            Assert.Null(settings.ProblemDeserializer);
        }

        #endregion

        #region Custom ProblemDeserializer

        [Fact]
        public async Task SendAsync_CustomProblemDeserializer_UsedOnError()
        {
            var customProblem = new ProblemDetails("custom:type", "Custom Error", 400);
            var deserializer = Substitute.For<IProblemDeserializer>();
            deserializer.DeserializeAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>())
                .Returns(Task.FromResult(customProblem));

            _settings.ProblemDeserializer = deserializer;
            SetupHttpResponse(HttpStatusCode.BadRequest, "any content");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertProblemDetailsException(result);
            Assert.Equal("custom:type", ex.Problem.Type);
            Assert.Equal("Custom Error", ex.Problem.Title);
            await deserializer.Received().DeserializeAsync(
                Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>());
        }

        [Fact]
        public async Task SendAsync_CustomProblemDeserializer_NotUsedOnSuccess()
        {
            var deserializer = Substitute.For<IProblemDeserializer>();
            _settings.ProblemDeserializer = deserializer;
            SetupHttpResponse(HttpStatusCode.OK, @"{ ""id"": 1, ""name"": ""test"" }");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            result.EnsureSuccessStatusCode();
            await deserializer.DidNotReceive().DeserializeAsync(
                Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>());
        }

        [Fact]
        public async Task SendAsync_CustomProblemDeserializer_ExceptionFallsBackToRequestException()
        {
            var deserializer = Substitute.For<IProblemDeserializer>();
            deserializer.DeserializeAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>())
                .Returns<ProblemDetails>(_ => throw new System.InvalidOperationException("parse failed"));

            _settings.ProblemDeserializer = deserializer;
            SetupHttpResponse(HttpStatusCode.BadRequest, "bad content");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            var ex = AssertExceptionFromResponse<RequestException>(result);
            Assert.IsNotType<ProblemDetailsException>(ex);
        }

        #endregion
    }
}
