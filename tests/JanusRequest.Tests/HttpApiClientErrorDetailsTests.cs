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

        #region RawResponse

        [Fact]
        public async Task SendAsync_CaptureRawResponseDisabled_RawResponseIsNull()
        {
            _settings.CaptureRawResponse = false;
            SetupHttpResponse(HttpStatusCode.BadRequest, ProblemDetailsJson);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.RawResponse);
        }

        [Fact]
        public async Task SendAsync_CaptureRawResponseEnabled_ErrorResponse_ContainsBody()
        {
            _settings.CaptureRawResponse = true;
            SetupHttpResponse(HttpStatusCode.BadRequest, ProblemDetailsJson);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.NotNull(result.RawResponse);
            Assert.Contains("Validation Error", result.RawResponse);
        }

        [Fact]
        public async Task SendAsync_CaptureRawResponseEnabled_SuccessResponse_RawResponseIsNull()
        {
            _settings.CaptureRawResponse = true;
            SetupHttpResponse(HttpStatusCode.OK, @"{ ""id"": 1, ""name"": ""test"" }");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.RawResponse);
        }

        [Fact]
        public async Task SendRequestAsync_CaptureRawResponseEnabled_ErrorResponse_ContainsBody()
        {
            _settings.CaptureRawResponse = true;
            SetupHttpResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

            var result = await _httpApiClient.SendRequestAsync(null,
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Equal("Internal Server Error", result.RawResponse);
        }

        #endregion

        #region ProblemDetails

        [Fact]
        public async Task SendAsync_ErrorWithProblemDetails_ParsesProblem()
        {
            SetupHttpResponse(HttpStatusCode.UnprocessableEntity, ProblemDetailsJson);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.NotNull(result.Problem);
            Assert.Equal("https://example.com/validation", result.Problem.Type);
            Assert.Equal("Validation Error", result.Problem.Title);
            Assert.Equal(422, result.Problem.Status);
            Assert.Equal("One or more fields are invalid.", result.Problem.Detail);
            Assert.Equal("/users/register", result.Problem.Instance);
            Assert.Equal("abc-123", result.Problem.Extensions["traceId"].Value);
        }

        [Fact]
        public async Task SendAsync_ErrorWithNonProblemDetails_ProblemIsNull()
        {
            SetupHttpResponse(HttpStatusCode.InternalServerError, "plain text error");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.Problem);
        }

        [Fact]
        public async Task SendAsync_SuccessResponse_ProblemIsNull()
        {
            SetupHttpResponse(HttpStatusCode.OK, @"{ ""id"": 1, ""name"": ""test"" }");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.Problem);
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

            Assert.NotNull(result.Problem);
            var errors = result.Problem.Extensions["errors"];
            Assert.True(errors.HasChildren);
            Assert.Equal("name", errors.Children["0"].Children["field"].Value);
            Assert.Equal("required", errors.Children["0"].Children["message"].Value);
        }

        [Fact]
        public async Task SendRequestAsync_ErrorWithProblemDetails_ParsesProblem()
        {
            SetupHttpResponse(HttpStatusCode.BadRequest, ProblemDetailsJson);

            var result = await _httpApiClient.SendRequestAsync(null,
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.NotNull(result.Problem);
            Assert.Equal("Validation Error", result.Problem.Title);
        }

        [Fact]
        public async Task SendRequestAsync_SuccessResponse_ProblemIsNull()
        {
            SetupHttpResponse(HttpStatusCode.OK, @"{ ""id"": 1 }");

            var result = await _httpApiClient.SendRequestAsync(null,
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.Problem);
        }

        [Fact]
        public async Task SendAsync_NoContent_ProblemAndRawResponseAreNull()
        {
            SetupHttpResponse(HttpStatusCode.NoContent, null);

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "DELETE", Path = "/test" });

            Assert.Null(result.Problem);
            Assert.Null(result.RawResponse);
        }

        #endregion

        #region Defaults

        [Fact]
        public void CaptureRawResponse_DefaultIsFalse()
        {
            var settings = new HttpApiClientSettings();
            Assert.False(settings.CaptureRawResponse);
        }

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

            Assert.NotNull(result.Problem);
            Assert.Equal("custom:type", result.Problem.Type);
            Assert.Equal("Custom Error", result.Problem.Title);
            await deserializer.Received(1).DeserializeAsync(
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

            Assert.Null(result.Problem);
            await deserializer.DidNotReceive().DeserializeAsync(
                Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>());
        }

        [Fact]
        public async Task SendAsync_CustomProblemDeserializer_ExceptionIgnored()
        {
            var deserializer = Substitute.For<IProblemDeserializer>();
            deserializer.DeserializeAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>())
                .Returns<ProblemDetails>(x => throw new InvalidOperationException("parse failed"));

            _settings.ProblemDeserializer = deserializer;
            SetupHttpResponse(HttpStatusCode.BadRequest, "bad content");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.Problem);
        }

        [Fact]
        public async Task SendAsync_CustomProblemDeserializer_WithCaptureRawResponse_ExceptionIgnored()
        {
            var deserializer = Substitute.For<IProblemDeserializer>();
            deserializer.DeserializeAsync(Arg.Any<HttpResponseMessage>(), Arg.Any<HttpApiClientSettings>())
                .Returns<ProblemDetails>(x => throw new InvalidOperationException("parse failed"));

            _settings.CaptureRawResponse = true;
            _settings.ProblemDeserializer = deserializer;
            SetupHttpResponse(HttpStatusCode.BadRequest, "bad content");

            var result = await _httpApiClient.SendAsync<TestResponse>(
                new HttpRequestInfo { Method = "GET", Path = "/test" });

            Assert.Null(result.Problem);
            Assert.Equal("bad content", result.RawResponse);
        }

        #endregion
    }
}
