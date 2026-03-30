using System.Net;
using System.Net.Http;
using JanusRequest.Integration.Tests.Fixtures;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Tests;

[Collection("IntegrationTests")]
public class LoggerAndTimeoutTests
{
    private readonly TestServerFixture _fixture;

    public LoggerAndTimeoutTests(TestServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpApiClient CreateClient() => new(_fixture.BaseUrl);

    // -------------------------------------------------------------------------
    // Per-request timeout
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PerRequestTimeout_CancelsSlowRequest()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo
        {
            Path = "/api/slow",
            Method = "GET",
            Timeout = TimeSpan.FromSeconds(1)
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetAsync<ItemResponse>(info));
    }

    [Fact]
    public async Task PerRequestTimeout_DoesNotAffectFastRequests()
    {
        using var client = CreateClient();
        var info = new HttpRequestInfo
        {
            Path = "/api/items/1",
            Method = "GET",
            Timeout = TimeSpan.FromSeconds(30)
        };

        var response = await client.GetAsync<ItemResponse>(info);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(1, response.Data.Id);
    }

    // -------------------------------------------------------------------------
    // Multiple loggers via Settings
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Logger_AddedToSettings_LogRequestCalled()
    {
        var logger = new RecordingLogger();
        var settings = new HttpApiClientSettings();
        settings.AddLogger(logger);

        using var client = CreateClient();
        client.Settings = settings;

        var request = new HeaderTestRequest { CustomHeader = "test", TraceId = "trace-001" };
        await client.PostAsync(request);

        Assert.True(logger.RequestCount > 0, "LogRequest was not called.");
        Assert.True(logger.ResponseCount > 0, "LogResponse was not called.");
        Assert.True(logger.LastElapsed > TimeSpan.Zero, "Elapsed time should be greater than zero.");
    }

    [Fact]
    public async Task MultipleLoggers_AllCalled()
    {
        var logger1 = new RecordingLogger();
        var logger2 = new RecordingLogger();

        var settings = new HttpApiClientSettings();
        settings.AddLogger(logger1);
        settings.AddLogger(logger2);

        using var client = CreateClient();
        client.Settings = settings;

        var request = new HeaderTestRequest { CustomHeader = "multi", TraceId = "trace-002" };
        await client.PostAsync(request);

        Assert.True(logger1.RequestCount > 0, "Logger1 LogRequest was not called.");
        Assert.True(logger2.RequestCount > 0, "Logger2 LogRequest was not called.");
        Assert.True(logger1.ResponseCount > 0, "Logger1 LogResponse was not called.");
        Assert.True(logger2.ResponseCount > 0, "Logger2 LogResponse was not called.");
    }

    [Fact]
    public async Task InstanceLogger_AndSettingsLoggers_BothCalled()
    {
        var settingsLogger = new RecordingLogger();
        var instanceLogger = new RecordingLogger();

        var settings = new HttpApiClientSettings();
        settings.AddLogger(settingsLogger);

        using var client = CreateClient();
        client.Settings = settings;
        client.Logger = instanceLogger;

        var request = new HeaderTestRequest { CustomHeader = "both", TraceId = "trace-003" };
        await client.PostAsync(request);

        Assert.True(settingsLogger.RequestCount > 0, "Settings logger was not called.");
        Assert.True(instanceLogger.RequestCount > 0, "Instance logger was not called.");
    }

    // -------------------------------------------------------------------------
    // Combined features: Header + Cookie + QueryArg + Body
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CombinedRequest_HeaderCookieQueryBody_AllSentCorrectly()
    {
        using var client = CreateClient();
        var request = new CombinedEchoRequest
        {
            CustomHeader = "header-value-123",
            SessionCookie = "session-token-abc",
            Tag = "integration-test",
            Value = "body-payload"
        };

        var response = await client.PostAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Data);

        Assert.True(response.Data.Headers.ContainsKey("X-Combined-Header"));
        Assert.Equal("header-value-123", response.Data.Headers["X-Combined-Header"]);

        Assert.True(response.Data.Cookies.ContainsKey("combined-session"));
        Assert.Equal("session-token-abc", response.Data.Cookies["combined-session"]);

        Assert.True(response.Data.QueryParams.ContainsKey("tag"));
        Assert.Equal("integration-test", response.Data.QueryParams["tag"]);

        Assert.Equal("body-payload", response.Data.BodyValue);
    }

    // -------------------------------------------------------------------------
    // Test logger implementation
    // -------------------------------------------------------------------------

    private sealed class RecordingLogger : IHttpApiClientLogger
    {
        public int RequestCount { get; private set; }
        public int ResponseCount { get; private set; }
        public int ErrorCount { get; private set; }
        public TimeSpan LastElapsed { get; private set; }

        public void LogRequest(HttpRequestMessage request) => RequestCount++;

        public void LogResponse(HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
        {
            ResponseCount++;
            LastElapsed = elapsed;
        }

        public void LogError(Exception exception, HttpRequestMessage request, HttpResponseMessage response) => ErrorCount++;
    }
}
