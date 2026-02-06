# JanusRequest

JanusRequest is a complete HTTP API client library for .NET Framework 4.5+ and modern .NET (net5.0, net7.0, net8.0).  
It provides a fluent API for building HTTP requests, automatic serialization/deserialization, error handling, and optional
integration with `IHttpClientFactory` and `Microsoft.Extensions.Logging`.

## Features

- **Fluent API design**: Chain methods for authentication, headers, and query parameters
- **Multiple content types**: JSON, XML, Form Data, Form URL-Encoded
- **Automatic Serialization/Deserialization**: Based on attributes and content types
- **Attribute-based configuration**: Decorate request types with routing and content metadata
- **Error handling**: built-in `HttpErrorHandler`, `RequestException`, `ThrottlingException`
- **Throttling support**: Automatic handling of HTTP 429 via `ThrottleRecoveryHandler`
- **Logging hook**: `IHttpApiClientLogger` interface and DI-based logger adapter
- **DI integration** (optional): `JanusRequest.Extensions.DependencyInjection` with `IHttpClientFactory`
- **JSON serializers**:
  - `System.Text.Json` by default on netstandard2.0+/net472+/net5.0+
  - `Newtonsoft.Json` only for legacy targets and via an optional package on .NET 5+
- **Media type resolution with structured suffixes**: E.g. `application/error+json` falls back to JSON
- **Non-standard body methods**: Opt-in support for GET/DELETE with body
- **Sync & async APIs**: Synchronous helpers built on top of async methods

---

## Installation

### Core library

```powershell
Install-Package JanusRequest
```

Targets: `net45; net472; netstandard2.0; net5.0+`.

### Optional: Dependency Injection integration

```powershell
Install-Package JanusRequest.Extensions.DependencyInjection
```

Targets: `net6.0+`.  
Provides extensions for `IServiceCollection` and integration with `IHttpClientFactory`.

### Optional: Newtonsoft.Json integration (.NET 5+)

```powershell
Install-Package JanusRequest.Json.Newtonsoft
```

Targets: `net5.0`.  
Allows you to switch JSON serialization from `System.Text.Json` back to `Newtonsoft.Json` on modern .NET.

---

## Quick Start

### Basic usage with `HttpApiClient`

```csharp
using JanusRequest;

// Create client
var client = new HttpApiClient("https://api.example.com");

// Simple GET request using HttpRequestInfo
var response = await client.SendAsync<UserResponse>(new HttpRequestInfo
{
    Method = "GET",
    Path = "/users/123"
});

Console.WriteLine($"User: {response.Data.Name}");
```

### Using Request Objects with Attributes

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/users/{id}")]
[ContentType(HttpContentType.Json)]
public class GetUserRequest : IRequestResponse<UserResponse>
{
    [PathOnly]
    public int Id { get; set; }

    [QueryArg("include")]
    public string Include { get; set; }
}

// Usage
var request = new GetUserRequest
{
    Id = 123,
    Include = "profile"
};

var response = await client.GetAsync(request);
```

### POST request with JSON body

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/users", Method = "POST")]
[ContentType(HttpContentType.Json)]
public class CreateUserRequest : IRequestResponse<UserResponse>
{
    public string Name { get; set; }
    public string Email { get; set; }

    [QueryIgnore] // Will not be included in query string
    public string Password { get; set; }
}

// Usage
var request = new CreateUserRequest
{
    Name = "John Doe",
    Email = "john@example.com",
    Password = "secret123"
};

var response = await client.PostAsync(request);
```

### Form-data upload

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/upload")]
[ContentType(HttpContentType.FormData)]
public class UploadRequest : IRequestResponse<UploadResponse>
{
    [FormData("file")]
    public Stream FileStream { get; set; }

    [FormData("description")]
    public string Description { get; set; }
}

// Usage
using var fileStream = File.OpenRead("document.pdf");

var request = new UploadRequest
{
    FileStream = fileStream,
    Description = "Important document"
};

var response = await client.PostAsync(request);
```

---

## Dependency Injection (optional)

The `JanusRequest.Extensions.DependencyInjection` package provides integration with `IServiceCollection`
and `IHttpClientFactory`, plus a factory abstraction for `HttpApiClient`.

### Registering a JanusRequest client

```csharp
using JanusRequest;
using JanusRequest.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddJanusRequestClient(
    configureClient: (provider, httpClient) =>
    {
        httpClient.BaseAddress = new Uri("https://api.example.com");
    },
    configureSettings: settings =>
    {
        settings.SetHandlers(
            new ThrottleRecoveryHandler(),
            new HttpErrorHandler());

        settings.LogResponseHeadersOnError = true;
    });
```

This registers:

- `HttpApiClient` as a typed client backed by `IHttpClientFactory`
- `HttpApiClientSettings` as a singleton
- `IHttpApiClientLogger` implemented by `LoggingHttpApiClientLogger`
- `IHttpApiClientFactory` to create `HttpApiClient` instances on demand

### Resolving the client

```csharp
public class MyService
{
    private readonly HttpApiClient client;

    public MyService(IHttpApiClientFactory factory)
    {
        client = factory.CreateClient();
    }

    public async Task<UserResponse> GetUserAsync(int id)
    {
        var request = new GetUserRequest { Id = id };
        var response = await client.GetAsync(request);
        return response.Data;
    }
}
```

You can also use the overload `AddJanusRequestClient(string name, ...)` to register multiple differently
configured clients using named `HttpClient` instances.

---

## Logging

The core library exposes an `IHttpApiClientLogger` interface:

```csharp
public interface IHttpApiClientLogger
{
    void LogRequest(HttpRequestMessage request);
    void LogResponse(HttpResponseMessage response);
    void LogError(Exception exception, HttpRequestMessage request, HttpResponseMessage response);
}
```

When using the DI package, `LoggingHttpApiClientLogger` adapts this interface to `ILogger<HttpApiClient>`,
logging:

- outgoing requests
- incoming responses
- errors (including optional response headers)

Control whether response headers are included in error logs using:

```csharp
var settings = new HttpApiClientSettings
{
    LogResponseHeadersOnError = true
};
```

---

## JSON serialization

### Default behavior

- On `net45`, JSON is handled by `Newtonsoft.Json`.
- On `netstandard2.0`, `net472` and `net5.0+`, JSON is handled by `System.Text.Json` in `JsonContentTranslator`.

The JSON translator automatically:

- serializes request bodies to `"application/json"`
- deserializes responses to your `TResponse` type
- ignores properties marked with REST attributes that should not be in the body

### Switching to Newtonsoft.Json on .NET 5+

Install the optional package:

```powershell
Install-Package JanusRequest.Json.Newtonsoft
```

Then configure your settings:

```csharp
using JanusRequest;

var settings = new HttpApiClientSettings()
    .UseNewtonsoftJson();

var client = new HttpApiClient("https://api.example.com")
{
    Settings = settings
};
```

This replaces the JSON translator to use `Newtonsoft.Json` while keeping other translators (XML, form data)
unchanged.

---

## Authentication helpers

```csharp
using JanusRequest;

// Bearer token
var client = new HttpApiClient("https://api.example.com")
    .SetBearerAuthentication("your-jwt-token");

// Basic authentication
var client2 = new HttpApiClient("https://api.example.com")
    .SetBasicAuthentication("username", "password");

// API key in header
var client3 = new HttpApiClient("https://api.example.com")
    .SetApiKeyAuthentication("your-api-key", "X-API-Key");
```

---

## Error handling

Use `HttpErrorHandler` to map non-success HTTP responses to exceptions:

```csharp
var settings = new HttpApiClientSettings()
    .SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler());

var client = new HttpApiClient("https://api.example.com")
{
    Settings = settings
};

try
{
    var response = await client.GetAsync(request);
}
catch (RequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.Response}");
    Console.WriteLine($"Url: {ex.Url}");

    if (ex.Headers != null)
    {
        foreach (var header in ex.Headers)
            Console.WriteLine($"{header.Key}: {string.Join(\", \", header.Value)}");
    }
}
catch (ThrottlingException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter} seconds");
    Console.WriteLine($"Retry at: {ex.RetryAt}");
}
```

`RequestException` now exposes:

- `StatusCode`
- `Response`
- `Headers`
- `Url`

---

## Custom response deserializers

For advanced scenarios, you can plug a custom deserializer implementing `IResponseDeserializer<TResponse>`:

```csharp
using JanusRequest;

public class CustomDeserializer : IResponseDeserializer<CustomResponse>
{
    public async Task<CustomResponse> DeserializeAsync(
        HttpResponseMessage response,
        HttpApiClientSettings settings)
    {
        var content = await response.Content.ReadAsStringAsync();
        // Custom deserialization logic
        return System.Text.Json.JsonSerializer.Deserialize<CustomResponse>(content)!;
    }
}

[Request("/custom")]
public class CustomRequest : IRequestResponse<CustomResponse, CustomDeserializer>
{
    public string Data { get; set; }
}
```

---

## Configuration settings

`HttpApiClientSettings` controls content serialization, deserialization and handlers:

```csharp
using JanusRequest;

var settings = new HttpApiClientSettings()
    .SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler());

settings.DefaultMediaType = HttpContentType.Json;        // default content type
settings.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";         // DateTime formatting
settings.TimeFormat = "HH:mm:ss";                        // TimeSpan formatting
settings.ValidateRequest = true;                         // DataAnnotations validation
settings.LogResponseHeadersOnError = true;               // include headers in error logs

var client = new HttpApiClient("https://api.example.com")
{
    Settings = settings
};
```

### Global content translator registration

You can register custom translators globally by media type:

```csharp
using JanusRequest;
using JanusRequest.ContentTranslator;

HttpApiClientSettings.RegisterGlobalContentTranslator(
    "application/error+json",
    () => new MyCustomJsonTranslator());
```

This allows you to support vendor-specific or custom media types across your application.

### Structured suffix media types

JanusRequest resolves media types using structured suffixes. For example:

- If the response media type is `application/error+json` and you only registered `application/json`,
  the JSON translator will still be used.
- If you register a specific translator for `application/error+json`, it will be preferred over the
  plain `application/json` translator.

---

## Advanced request configuration

### Allowing body on GET / DELETE

Some APIs accept a body with GET or DELETE requests. This is not standard, but you can opt in:

```csharp
var info = new HttpRequestInfo
{
    Method = "GET",
    Path = "/search",
    AllowNonStandardBody = NonStandardBodyMethods.Get
};

var response = await client.SendAsync<SearchResponse>(info);
```

### Absolute URLs

If `HttpRequestInfo.Path` starts with `http://` or `https://`, it is treated as an absolute URL and the
client base address is ignored:

```csharp
var response = await client.SendAsync<UserResponse>(new HttpRequestInfo
{
    Method = "GET",
    Path = "https://other-api.example.com/users/123"
});
```

---

## Breaking changes from 1.0.2 to 2.0.0

The 2.0.0 release introduces some important behavioral and API changes compared to 1.0.2.

| Change | Impact | Migration |
|-------|--------|----------|
| `Path()` → `Patch()` | The synchronous `Path` extension method in `HttpClientExtension` was a typo for PATCH. | Replace `client.Path(request)` with `client.Patch(request)`. |
| `DefaultContentType` obsolete | `HttpApiClientSettings` now exposes `DefaultMediaType` as the preferred property. `DefaultContentType` is kept only for backward compatibility and marked `[Obsolete]`. | Use `settings.DefaultMediaType = HttpContentType.Json;`. |
| `HttpContentType` enum → static class | `HttpContentType` is now a static class with string constants (e.g. `"application/json"`). Existing code using `HttpContentType.Json` continues to work because it is still a public constant. | No change required in most cases. Treat values as `string` when needed. |
| `ContentTypeAttribute` uses string media type | `ContentTypeAttribute` now takes a raw media type string. Attributes like `[ContentType(HttpContentType.Json)]` keep working because `HttpContentType.Json` is a string constant. | For custom types, use `[ContentType("application/error+json")]`. |
| Default JSON serializer | On netstandard2.0+/net472+/net5.0+, the default JSON serializer switched from `Newtonsoft.Json` to `System.Text.Json`. Serialization behavior (naming, null handling, exceptions) may change. | To keep Newtonsoft behavior on .NET 5+, install `JanusRequest.Json.Newtonsoft` and call `settings.UseNewtonsoftJson()`. |
| `RequestException.Headers` | `RequestException` gained a `Headers` property with all response headers. Existing code still compiles, but you can now read headers when handling errors. | Optionally use `ex.Headers` when inspecting errors. |