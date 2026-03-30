# JanusRequest

[![NuGet](https://img.shields.io/nuget/v/JanusRequest.svg)](https://www.nuget.org/packages/JanusRequest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-net45%20%7C%20net472%20%7C%20netstandard2.0%20%7C%20net5%2B-512bd4)](https://dotnet.microsoft.com)

A typed HTTP client library for .NET with attribute-based routing, automatic serialization, fluent authentication, and built-in error recovery. Supports both sync and async APIs across .NET Framework 4.5+ and modern .NET.

> **⚠️ Upgrading from v1.x?** This version contains breaking changes. See the [Migration Guide: v1.x → v2.x](https://github.com/gustavorviana/JanusRequest/wiki/Migration-Guide:-v1.x-%E2%86%92-v2.x) before updating.

---

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Request Objects & Attributes](#request-objects--attributes)
- [HTTP Verbs](#http-verbs)
  - [GET without body](#get-without-body)
  - [Verb overloads with string URL](#verb-overloads-with-string-url)
  - [Form-data upload](#form-data-upload)
- [Authentication](#authentication)
- [Error Handling](#error-handling)
- [Non-Standard Body Methods](#non-standard-body-methods)
- [Custom Deserializers](#custom-deserializers)
- [Deserializer Cache](#deserializer-cache)
- [Logging](#logging)
- [JSON Serialization](#json-serialization)
- [Dependency Injection](#dependency-injection)
- [Breaking Changes (v1.x → v2.x)](#breaking-changes-v1x--v2x)

---

## Installation

| Package | Target | Purpose |
|---|---|---|
| `JanusRequest` | `net45; net472; netstandard2.0; net5.0; net7.0; net8.0` | Core library |
| `JanusRequest.Extensions.DependencyInjection` | `netstandard2.0; net5.0; net6.0; net8.0` | `IServiceCollection` + `IHttpClientFactory` integration |
| `JanusRequest.Json.Newtonsoft` | `net5.0; net6.0; net8.0` | `Newtonsoft.Json` support on modern .NET |

```powershell
Install-Package JanusRequest

# Optional
Install-Package JanusRequest.Extensions.DependencyInjection
Install-Package JanusRequest.Json.Newtonsoft
```

---

## Quick Start

```csharp
using JanusRequest;
using JanusRequest.Attributes;

// 1. Define a typed request
[Request("/users/{id}")]
[ContentType(HttpContentType.Json)]
public class GetUserRequest : IRequestResponse<UserResponse>
{
    [PathOnly]
    public int Id { get; set; }
}

public class UserResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// 2. Send it
using var client = new HttpApiClient("https://api.example.com");

var response = await client.GetAsync(new GetUserRequest { Id = 42 });
Console.WriteLine(response.Data.Name);
```

For bodyless GETs, a typed URL overload is also available:

```csharp
var response = await client.GetAsync<UserResponse>("/users/42");
```

---

## Request Objects & Attributes

Request classes implement `IRequestResponse<TResponse>` and are decorated with attributes that control routing, HTTP method, and content type. Properties are annotated to control how they map to the URL path, query string, or request body.

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/orders/{orderId}/items")]
[ContentType(HttpContentType.Json)]
public class GetOrderItemsRequest : IRequestResponse<OrderItemsResponse>
{
    [PathOnly]          // Substituted into the URL path; excluded from body and query string
    public int OrderId { get; set; }

    [QueryArg("page")]  // Appended to the query string as ?page=N
    public int Page { get; set; }

    [QueryArg("limit")]
    public int Limit { get; set; }
}
```

| Attribute | Target | Effect |
|---|---|---|
| `[Request(path)]` | Class | Defines default URL path and optional HTTP method |
| `[ContentType(...)]` | Class | Sets the `Content-Type` for the request body |
| `[PathOnly]` | Property | Used for path substitution; excluded from body and query string |
| `[QueryArg("name")]` | Property | Forces the property into the query string with the given name |
| `[QueryIgnore]` | Property | Excludes the property from the query string entirely |
| `[FormData("name")]` | Property | Maps the property as a named field in multipart form-data |

---

## HTTP Verbs

### GET without body

Use `GetAsync<TResponse>` when there is no request body. The overload accepting `HttpRequestInfo` always forces the method to `GET` internally, regardless of what `Method` is set to on the info object.

```csharp
using JanusRequest;

using var client = new HttpApiClient("https://api.example.com");

// Via string URL
var r1 = await client.GetAsync<UserResponse>("/users/42");

// Via HttpRequestInfo — useful when extra headers or query params are needed
var info = new HttpRequestInfo
{
    Path  = "/users/42",
    Query = new UrlQueryBuilder().Set("include", "profile")
};
var r2 = await client.GetAsync<UserResponse>(info);

// Synchronous equivalents (HttpClientExtension)
var s1 = client.Get<UserResponse>("/users/42");
var s2 = client.Get<UserResponse>(info);
```

### Verb overloads with string URL

All verbs that accept a request body also expose a `string url` overload, eliminating the need to construct an `HttpRequestInfo` solely to override the path.

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/default-path")]
[ContentType(HttpContentType.Json)]
public class ItemRequest : IRequestResponse<ItemResponse>
{
    public string Name { get; set; }
}

public class ItemResponse { public int Id { get; set; } }

using var client = new HttpApiClient("https://api.example.com");
var body = new ItemRequest { Name = "example" };

// Async
var r1 = await client.GetAsync(body,             "/api/items");
var r2 = await client.PostAsync(body,            "/api/items");
var r3 = await client.PutAsync(body,             "/api/items/1");
var r4 = await client.DeleteAsync(body,          "/api/items/1");
var r5 = await client.PatchAsync(body,           "/api/items/1");
var r6 = await client.SendAsync("OPTIONS", body, "/api/items");

// Synchronous equivalents
var s1 = client.Post(body,         "/api/items");
var s2 = client.Put(body,          "/api/items/1");
var s3 = client.Delete(body,       "/api/items/1");
var s4 = client.Patch(body,        "/api/items/1");
var s5 = client.Send("PUT", body,  "/api/items/1");
```

### Form-data upload

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[Request("/files")]
[ContentType(HttpContentType.FormData)]
public class UploadRequest : IRequestResponse<UploadResponse>
{
    [FormData("file")]
    public Stream FileStream { get; set; }

    [FormData("description")]
    public string Description { get; set; }
}

public class UploadResponse { public string FileId { get; set; } }

using var client = new HttpApiClient("https://api.example.com");
using var stream  = File.OpenRead("report.pdf");

var response = await client.PostAsync(new UploadRequest
{
    FileStream  = stream,
    Description = "Q3 Report"
});
```

---

## Authentication

All helpers return `this`, supporting method chaining.

```csharp
using JanusRequest;

// Bearer token (OAuth 2.0 / JWT)
using var client = new HttpApiClient("https://api.example.com")
    .SetBearerAuthentication("eyJhbGci...");

// HTTP Basic
using var client2 = new HttpApiClient("https://api.example.com")
    .SetBasicAuthentication("username", "password");

// API key (custom header)
using var client3 = new HttpApiClient("https://api.example.com")
    .SetApiKeyAuthentication("my-api-key", "X-API-Key");

// Custom scheme
using var client4 = new HttpApiClient("https://api.example.com")
    .SetAuthentication("Digest", "realm-value");

// Remove authentication
client.ClearAuthentication();
```

---

## Error Handling

`HttpErrorHandler` maps non-success HTTP responses to `RequestException`. `ThrottleRecoveryHandler` automatically waits for the `Retry-After` period and retries on HTTP 429. Both implement `IHttpHandlerBase` and are composed via `SetHandlers`.

```csharp
using JanusRequest;
using JanusRequest.HttpHandlers;

var settings = new HttpApiClientSettings()
    .SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler());

using var client = new HttpApiClient("https://api.example.com") { Settings = settings };

try
{
    var response = await client.GetAsync<UserResponse>("/users/42");
    Console.WriteLine(response.Data.Name);
}
catch (RequestException ex)
{
    // Carries StatusCode, response body, request URL, and response headers
    Console.WriteLine($"[{(int)ex.StatusCode}] {ex.Url}");
    Console.WriteLine(ex.Response);

    foreach (var header in ex.Headers ?? [])
        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
}
catch (ThrottlingException ex)
{
    Console.WriteLine($"Rate limited — retry after {ex.RetryAfter}s (at {ex.RetryAt:u})");
}
```

To include response headers in error logs:

```csharp
var settings = new HttpApiClientSettings
{
    LogResponseHeadersOnError = true
};
```

---

## Non-Standard Body Methods

By default, `GET` and `DELETE` requests do not include a body. Use `NonStandardBodyMethods` on `HttpRequestInfo` to opt in per-request.

```csharp
using JanusRequest;
using JanusRequest.Attributes;

[ContentType(HttpContentType.Json)]
public class SearchRequest : IRequestResponse<SearchResponse>
{
    public string Query { get; set; }
    public string[] Filters { get; set; }
}

public class SearchResponse { public int TotalCount { get; set; } }

using var client = new HttpApiClient("https://api.example.com");

var info = new HttpRequestInfo
{
    Path = "/search",
    AllowNonStandardBody = NonStandardBodyMethods.Get
};

var response = await client.GetAsync(new SearchRequest { Query = "laptop" }, info);
```

`NonStandardBodyMethods` is a `[Flags]` enum:

| Value | Effect |
|---|---|
| `None` | Default — no body on GET or DELETE |
| `Get` | Allows body on GET |
| `Delete` | Allows body on DELETE |
| `All` | Allows body on both GET and DELETE |

---

## Custom Deserializers

For responses that require special deserialization logic, implement `IResponseDeserializer<TResponse>` and attach it to a request type or response type.

### Via `IRequestResponse<TResponse, TDeserializer>`

```csharp
using JanusRequest;
using System.Net.Http;
using System.Text.Json;

public class UserResponse { public int Id { get; set; } public string Name { get; set; } }

public class UserDeserializer : IResponseDeserializer<UserResponse>
{
    public async Task<UserResponse> DeserializeAsync(
        HttpResponseMessage response, HttpApiClientSettings settings)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<UserResponse>(json);
    }
}

[Request("/users/{id}")]
public class GetUserRequest : IRequestResponse<UserResponse, UserDeserializer>
{
    [PathOnly]
    public int Id { get; set; }
}
```

### Via `[ResponseDeserializer]`

```csharp
using JanusRequest.Attributes;

[ResponseDeserializer(typeof(UserDeserializer))]
public class UserResponse { public int Id { get; set; } }
```

### Via `HttpApiClientSettings.AddDeserializer`

```csharp
var settings = new HttpApiClientSettings();
settings.AddDeserializer<UserResponse, UserDeserializer>();

using var client = new HttpApiClient("https://api.example.com") { Settings = settings };
```

---

## Deserializer Cache

`HttpApiClientSettings` caches the reflection results used to resolve deserializer types. The cache is per-settings-instance — different settings objects maintain independent caches, consistent with the library's existing isolation model. There is no action required to benefit from it.

```csharp
var settings = new HttpApiClientSettings();

// Cache is populated automatically on first call to GetDeserializerType.
// Manual reset is available for hot-reload or test isolation scenarios.
// Returns 'this' for fluent chaining.
settings.ClearDeserializerTypeCache();
```

---

## Logging

Implement `IHttpApiClientLogger` to hook into the request/response/error lifecycle without a hard dependency on any specific logging framework.

```csharp
using JanusRequest;
using System;
using System.Net.Http;

public class ConsoleLogger : IHttpApiClientLogger
{
    public void LogRequest(HttpRequestMessage request)
        => Console.WriteLine($"→ {request.Method} {request.RequestUri}");

    public void LogResponse(HttpResponseMessage response)
        => Console.WriteLine($"← {(int)response.StatusCode} {response.ReasonPhrase}");

    public void LogError(Exception ex, HttpRequestMessage request, HttpResponseMessage response)
        => Console.Error.WriteLine($"✗ {ex.Message}");
}

using var client = new HttpApiClient("https://api.example.com")
{
    Logger = new ConsoleLogger()
};
```

When using the DI package, `LoggingHttpApiClientLogger` automatically adapts `IHttpApiClientLogger` to `ILogger<HttpApiClient>`.

---

## JSON Serialization

### Default behavior

| Target framework | Serializer |
|---|---|
| `net45` | `Newtonsoft.Json` |
| `net472`, `netstandard2.0`, `net5.0`, `net6.0`, `net8.0` | `System.Text.Json` |

### Switch to Newtonsoft.Json on .NET 5+

```csharp
using JanusRequest;

var settings = new HttpApiClientSettings().UseNewtonsoftJson();

using var client = new HttpApiClient("https://api.example.com") { Settings = settings };
```

### Register a global content translator

Overrides apply to all `HttpApiClientSettings` instances created after registration. Pass `null` as the factory to remove an override.

```csharp
HttpApiClientSettings.RegisterGlobalContentTranslator(
    HttpContentType.Json,
    () => new MyCustomJsonTranslator());
```

---

## Dependency Injection

> Requires `JanusRequest.Extensions.DependencyInjection` (`netstandard2.0; net5.0; net6.0; net8.0`)

### Default client

`AddJanusRequestClient` registers the core services and returns an `IHttpClientBuilder` for configuring the underlying `HttpClient`:

```csharp
using JanusRequest;
using JanusRequest.Extensions.DependencyInjection;
using JanusRequest.HttpHandlers;
using Microsoft.Extensions.DependencyInjection;

services
    .AddJanusRequestClient(settings =>
    {
        settings.SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler());
        settings.LogResponseHeadersOnError = true;
    })
    .ConfigureHttpClient((provider, httpClient) =>
    {
        httpClient.BaseAddress = new Uri("https://api.example.com");
    });
```

The extension registers:
- `HttpApiClientSettings` as a singleton
- `IHttpApiClientLogger` implemented by `LoggingHttpApiClientLogger`
- `IHttpApiClientFactory` for on-demand client creation
- `HttpApiClient` as a typed client backed by `IHttpClientFactory`

```csharp
public class UserService
{
    private readonly HttpApiClient _client;

    public UserService(IHttpApiClientFactory factory)
        => _client = factory.CreateClient();

    public async Task<UserResponse> GetAsync(int id, CancellationToken ct = default)
    {
        var response = await _client.GetAsync(new GetUserRequest { Id = id }, ct);
        return response.Data;
    }
}
```

### Named clients

Use `AddJanusRequestClient(name, configureClient)` when you need multiple clients with different configurations. The `configureClient` action receives the resolved `IServiceProvider` and the `HttpApiClient` instance after it is created — use it to apply per-client configuration such as authentication:

```csharp
services
    .AddJanusRequestClient(settings =>
    {
        settings.SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler());
    })
    .ConfigureHttpClient((provider, httpClient) =>
        httpClient.BaseAddress = new Uri("https://api.example.com"));

services
    .AddJanusRequestClient("payments", (provider, client) =>
    {
        var token = provider.GetRequiredService<ITokenService>().GetToken();
        client.SetBearerAuthentication(token);
    })
    .ConfigureHttpClient((provider, httpClient) =>
        httpClient.BaseAddress = new Uri("https://payments.example.com"));

services
    .AddJanusRequestClient("notifications", (provider, client) =>
    {
        client.SetApiKeyAuthentication("my-api-key", "X-API-Key");
    })
    .ConfigureHttpClient((provider, httpClient) =>
        httpClient.BaseAddress = new Uri("https://notifications.example.com"));
```

`factory.CreateClient(name)` passes the name directly to the underlying `IHttpClientFactory`, so `"payments"` in `AddJanusRequestClient` maps 1-to-1 to `"payments"` in `CreateClient`. The `configureClient` action registered for that name is then applied to the resulting `HttpApiClient`:

```csharp
public class PaymentService
{
    private readonly HttpApiClient _client;

    public PaymentService(IHttpApiClientFactory factory)
        => _client = factory.CreateClient("payments");
}

public class NotificationService
{
    private readonly HttpApiClient _client;

    public NotificationService(IHttpApiClientFactory factory)
        => _client = factory.CreateClient("notifications");
}
```

Calling `factory.CreateClient()` without arguments, or with a `null`/empty string, resolves the default client.

---

## Breaking Changes (v1.x → v2.x)

> For a detailed migration guide with code examples, see [MIGRATION.md](MIGRATION.md).

| Change | v1.x | v2.x |
|---|---|---|
| **Project structure** | `JanusRequest/` at root | `src/JanusRequest/` |
| **`HttpApiClient` implements `IHttpApiClient`** | No interface, class only | Implements `IHttpApiClient`; methods are `virtual` |
| **Sync extension methods target `IHttpApiClient`** | `this HttpApiClient` | `this IHttpApiClient` |
| **`IRequestResponse<T, TDeserializer>` deprecated** | Primary API | Marked `[Obsolete]` — use `[ResponseDeserializer]` attribute on response type instead |
| **`HttpApiClient.GetDeserializerType` deprecated** | Static method on client | Marked `[Obsolete]` — use `HttpApiClientSettings.GetDeserializerType` |
| **`DefaultContentType` renamed** | `settings.DefaultContentType` | `settings.DefaultMediaType` (`DefaultContentType` kept as `[Obsolete]` proxy) |
| **`AddJanusRequestClient` signature changed** | `AddJanusRequestClient(configureClient, configureSettings)` | `AddJanusRequestClient(configureSettings)` — configure `HttpClient` via `.ConfigureHttpClient()` on the returned `IHttpClientBuilder` |
| **Named clients in DI** | Not supported | `AddJanusRequestClient(name, configureClient)` with `IHttpApiClientFactory.CreateClient(name)` |
| **`DeserializationException` added** | Deserialization errors thrown as raw exceptions | Wrapped in `DeserializationException` (extends `RequestException`) with `Content`, `TargetType`, and `InnerException` |
| **`RequestException` new constructor** | No `innerException` overload | Added `RequestException(statusCode, response, innerException)` |
| **New attributes** | — | `[Header]`, `[HeaderCollection]`, `[Cookie]`, `[CookieCollection]`, `[ResponseDeserializer]` |
| **Query string behavior for DELETE** | DELETE sends all properties as query args | DELETE now treated like POST/PUT (only `[QueryArg]` properties go to query string) unless `AllowNonStandardBody` is set |
| **`net45` removed from DI package** | DI package targeted `net45` | DI package targets `netstandard2.0; net5.0; net6.0; net8.0` only |