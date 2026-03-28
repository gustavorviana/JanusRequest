using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using JanusRequest.Integration.Tests.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JanusRequest.Integration.Tests.Fixtures;

public class TestServerFixture : IAsyncLifetime
{
    public string BaseUrl { get; private set; } = default!;
    private WebApplication _app = default!;

    private readonly ConcurrentDictionary<string, int> _callCounters = new();

    public int GetAndIncrementCallCount(string key)
        => _callCounters.AddOrUpdate(key, 1, (_, v) => v + 1);

    public void ResetCallCount(string key)
        => _callCounters.TryRemove(key, out _);

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = null; // PascalCase to match .NET property names
        });
        _app = builder.Build();
        MapEndpoints(_app);
        await _app.StartAsync();
        BaseUrl = _app.Urls.First();
    }

    public async Task DisposeAsync()
    {
        await _app.DisposeAsync();
    }

    private void MapEndpoints(WebApplication app)
    {
        MapCrudEndpoints(app);
        MapAuthEndpoints(app);
        MapAuthenticatorEndpoints(app);
        MapEchoEndpoints(app);
        MapFileEndpoints(app);
        MapRetryEndpoints(app);
        MapErrorEndpoints(app);
        MapContentTypeEndpoints(app);
        MapBase64Endpoints(app);
        MapTimeoutEndpoints(app);
        MapCombinedEchoEndpoint(app);
    }

    private static void MapCrudEndpoints(WebApplication app)
    {
        var items = new List<ItemResponse>
        {
            new() { Id = 1, Name = "Item1" },
            new() { Id = 2, Name = "Item2" },
            new() { Id = 3, Name = "Item3" }
        };

        app.MapGet("/api/items", () => Results.Ok(items));

        app.MapGet("/api/items/{id:int}", (int id) =>
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            return item is not null ? Results.Ok(item) : Results.NotFound(new ErrorResponse { Error = "Not found" });
        });

        app.MapPost("/api/items", async (HttpContext ctx) =>
        {
            var body = await ctx.Request.ReadFromJsonAsync<CreateItemBody>();
            var newItem = new ItemResponse { Id = items.Count + 1, Name = body?.Name };
            return Results.Created($"/api/items/{newItem.Id}", newItem);
        });

        app.MapPut("/api/items/{id:int}", async (int id, HttpContext ctx) =>
        {
            var body = await ctx.Request.ReadFromJsonAsync<UpdateItemBody>();
            return Results.Ok(new ItemResponse { Id = id, Name = body?.Name });
        });

        app.MapDelete("/api/items/{id:int}", (int id) => Results.NoContent());

        app.MapPatch("/api/items/{id:int}", async (int id, HttpContext ctx) =>
        {
            var body = await ctx.Request.ReadFromJsonAsync<UpdateItemBody>();
            return Results.Ok(new ItemResponse { Id = id, Name = body?.Name });
        });
    }

    private static void MapAuthEndpoints(WebApplication app)
    {
        app.MapGet("/api/auth/bearer", (HttpContext ctx) =>
        {
            var authHeader = ctx.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.Unauthorized();

            var token = authHeader["Bearer ".Length..];
            return Results.Ok(new AuthInfoResponse { Scheme = "Bearer", Token = token });
        });

        app.MapGet("/api/auth/basic", (HttpContext ctx) =>
        {
            var authHeader = ctx.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
                return Results.Unauthorized();

            var encoded = authHeader["Basic ".Length..];
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':', 2);
            return Results.Ok(new AuthInfoResponse { Scheme = "Basic", User = parts[0], Pass = parts.Length > 1 ? parts[1] : null });
        });

        app.MapGet("/api/auth/apikey", (HttpContext ctx) =>
        {
            var apiKey = ctx.Request.Headers["X-API-Key"].ToString();
            if (string.IsNullOrEmpty(apiKey))
            {
                var customKey = ctx.Request.Headers["Authorization-Key"].ToString();
                if (string.IsNullOrEmpty(customKey))
                    return Results.Unauthorized();
                return Results.Ok(new AuthInfoResponse { ApiKey = customKey });
            }

            return Results.Ok(new AuthInfoResponse { ApiKey = apiKey });
        });
    }

    private void MapAuthenticatorEndpoints(WebApplication app)
    {
        // Returns the auth token echo if a valid Bearer token is present.
        // Used to verify that IHttpAuthenticator.AuthenticateAsync is called before the request.
        app.MapGet("/api/authenticator/protected", (HttpContext ctx) =>
        {
            var authHeader = ctx.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.Unauthorized();

            var token = authHeader["Bearer ".Length..];
            return Results.Ok(new AuthInfoResponse { Scheme = "Bearer", Token = token });
        });

        // Simulates a token-refresh scenario.
        // The first call for a given key returns 401; subsequent calls with the refreshed token ("refreshed-token") succeed.
        app.MapPost("/api/authenticator/token-refresh", (HttpContext ctx) =>
        {
            var key = ctx.Request.Query["key"].ToString();
            var count = GetAndIncrementCallCount($"token-refresh-{key}");

            // First call always returns 401 to force the authenticator to refresh
            if (count == 1)
                return Results.Unauthorized();

            var authHeader = ctx.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.Unauthorized();

            var token = authHeader["Bearer ".Length..];
            return Results.Ok(new AuthInfoResponse { Scheme = "Bearer", Token = token });
        });

        // Simulates a forbidden scenario that can be resolved by re-authentication.
        // First call returns 403; subsequent calls with valid token succeed.
        app.MapPost("/api/authenticator/forbidden-refresh", (HttpContext ctx) =>
        {
            var key = ctx.Request.Query["key"].ToString();
            var count = GetAndIncrementCallCount($"forbidden-refresh-{key}");

            if (count == 1)
                return Results.StatusCode(403);

            var authHeader = ctx.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.StatusCode(403);

            var token = authHeader["Bearer ".Length..];
            return Results.Ok(new AuthInfoResponse { Scheme = "Bearer", Token = token });
        });
    }

    private static void MapEchoEndpoints(WebApplication app)
    {
        app.MapPost("/api/echo/headers", (HttpContext ctx) =>
        {
            var headers = new Dictionary<string, string>();
            foreach (var header in ctx.Request.Headers)
                headers[header.Key] = header.Value.ToString();

            return Results.Ok(new EchoResponse { Values = headers });
        });

        app.MapPost("/api/echo/cookies", (HttpContext ctx) =>
        {
            var cookies = new Dictionary<string, string>();
            foreach (var cookie in ctx.Request.Cookies)
                cookies[cookie.Key] = cookie.Value;

            return Results.Ok(new EchoResponse { Values = cookies });
        });

        app.MapGet("/api/echo/query", (HttpContext ctx) =>
        {
            var queryParams = new Dictionary<string, string>();
            foreach (var param in ctx.Request.Query)
                queryParams[param.Key] = param.Value.ToString();

            return Results.Ok(new EchoResponse { Values = queryParams });
        });
    }

    private static void MapFileEndpoints(WebApplication app)
    {
        app.MapPost("/api/files/upload", async (HttpContext ctx) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            var fields = new Dictionary<string, string>();

            foreach (var field in form)
            {
                if (field.Key != "file")
                    fields[field.Key] = field.Value.ToString();
            }

            long size = 0;
            string? fileName = null;

            if (file is not null)
            {
                size = file.Length;
                fileName = file.FileName;
            }

            return Results.Ok(new FileUploadResponse
            {
                FileName = fileName,
                Size = size,
                Fields = fields
            });
        });

        app.MapGet("/api/files/download", () =>
        {
            var bytes = new byte[256];
            for (var i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(i % 256);

            return Results.File(bytes, "application/octet-stream", "testfile.bin");
        });
    }

    private void MapRetryEndpoints(WebApplication app)
    {
        app.MapGet("/api/retry/throttle", (HttpContext ctx) =>
        {
            var key = ctx.Request.Query["key"].ToString();
            var failUntil = int.Parse(ctx.Request.Query["failUntil"].FirstOrDefault() ?? "2");
            var count = GetAndIncrementCallCount($"throttle-{key}");

            if (count <= failUntil)
            {
                ctx.Response.Headers["Retry-After"] = "0";
                return Results.StatusCode(429);
            }

            return Results.Ok(new ItemResponse { Id = 1, Name = "Success" });
        });

        app.MapGet("/api/retry/transient", (HttpContext ctx) =>
        {
            var key = ctx.Request.Query["key"].ToString();
            var failUntil = int.Parse(ctx.Request.Query["failUntil"].FirstOrDefault() ?? "2");
            var count = GetAndIncrementCallCount($"transient-{key}");

            if (count <= failUntil)
                return Results.StatusCode(503);

            return Results.Ok(new ItemResponse { Id = 1, Name = "Success" });
        });
    }

    private static void MapErrorEndpoints(WebApplication app)
    {
        app.MapGet("/api/errors/not-found", () =>
            Results.NotFound(new ErrorResponse { Error = "Not found" }));

        app.MapGet("/api/errors/server-error", () =>
            Results.Json(new ErrorResponse { Error = "Internal server error" }, statusCode: 500));

        app.MapGet("/api/errors/throttled", (HttpContext ctx) =>
        {
            ctx.Response.Headers["Retry-After"] = "5";
            ctx.Response.Headers["X-RateLimit-Limit"] = "100";
            return Results.StatusCode(429);
        });

        app.MapGet("/api/errors/bad-json", async (HttpContext ctx) =>
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = 200;
            await ctx.Response.WriteAsync("not valid json{{{");
        });
    }

    private static void MapContentTypeEndpoints(WebApplication app)
    {
        app.MapPost("/api/content/xml", async (HttpContext ctx) =>
        {
            var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
            var serializer = new XmlSerializer(typeof(XmlItemResponse));

            XmlItemResponse? item;
            using (var reader = new StringReader(body))
                item = (XmlItemResponse?)serializer.Deserialize(reader);

            ctx.Response.ContentType = "application/xml";
            using var writer = new StringWriter();
            serializer.Serialize(writer, item);
            await ctx.Response.WriteAsync(writer.ToString());
        });

        app.MapPost("/api/content/form-urlencoded", async (HttpContext ctx) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var fields = new Dictionary<string, string>();
            foreach (var field in form)
                fields[field.Key] = field.Value.ToString();

            await ctx.Response.WriteAsJsonAsync(new FormEchoResponse { Fields = fields });
        });
    }

    private static void MapBase64Endpoints(WebApplication app)
    {
        app.MapPost("/api/content/base64-json", async (HttpContext ctx) =>
        {
            var body = await ctx.Request.ReadFromJsonAsync<Base64JsonBody>();
            // Echo back the same data — the Image field arrives as base64 string in JSON,
            // System.Text.Json on the server deserializes byte[] from base64 natively
            await ctx.Response.WriteAsJsonAsync(new
            {
                Name = body?.Name,
                Image = body?.Image // Will be serialized back as base64 by STJ
            });
        });
    }

    private record Base64JsonBody(string? Name, byte[]? Image);

    private static void MapTimeoutEndpoints(WebApplication app)
    {
        app.MapGet("/api/slow", async (HttpContext ctx) =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), ctx.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            ctx.Response.StatusCode = 200;
            await ctx.Response.WriteAsync("done");
        });
    }

    private static void MapCombinedEchoEndpoint(WebApplication app)
    {
        app.MapPost("/api/echo/combined", async (HttpContext ctx) =>
        {
            var headers = new Dictionary<string, string>();
            foreach (var header in ctx.Request.Headers)
                headers[header.Key] = header.Value.ToString();

            var cookies = new Dictionary<string, string>();
            foreach (var cookie in ctx.Request.Cookies)
                cookies[cookie.Key] = cookie.Value;

            var queryParams = new Dictionary<string, string>();
            foreach (var param in ctx.Request.Query)
                queryParams[param.Key] = param.Value.ToString();

            var body = await ctx.Request.ReadFromJsonAsync<CombinedEchoBody>();

            await ctx.Response.WriteAsJsonAsync(new
            {
                Headers = headers,
                Cookies = cookies,
                QueryParams = queryParams,
                BodyValue = body?.Value
            });
        });
    }

    private record CombinedEchoBody(string? Value);

    // Internal DTOs for server-side deserialization (not shared with client)
    private record CreateItemBody(string? Name);
    private record UpdateItemBody(string? Name);
}
