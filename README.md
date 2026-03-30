# JanusRequest

[![NuGet](https://img.shields.io/nuget/v/JanusRequest.svg)](https://www.nuget.org/packages/JanusRequest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-net45%20%7C%20net472%20%7C%20netstandard2.0%20%7C%20net5%2B-512bd4)](https://dotnet.microsoft.com)

A typed HTTP client library for .NET with attribute-based routing, automatic serialization, fluent authentication, and built-in error recovery. Supports both sync and async APIs across .NET Framework 4.5+ and modern .NET.

> **Upgrading from v1.x?** See the [Migration Guide](https://github.com/gustavorviana/JanusRequest/wiki/Migration-Guide-v1.x-to-v2.x) before updating.

---

## Installation

```powershell
Install-Package JanusRequest
```

Optional packages:

```powershell
Install-Package JanusRequest.Extensions.DependencyInjection   # IServiceCollection + IHttpClientFactory
Install-Package JanusRequest.Json.Newtonsoft                   # Newtonsoft.Json on .NET 5+
```

---

## Quick Start

```csharp
using JanusRequest;
using JanusRequest.Attributes;

// Define a typed request
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

// Send it
using var client = new HttpApiClient("https://api.example.com");

var response = await client.GetAsync(new GetUserRequest { Id = 42 });
Console.WriteLine(response.Data.Name);

// Or use a simple URL overload
var response2 = await client.GetAsync<UserResponse>("/users/42");
```

---

## Features

| Feature | Description | Wiki |
|---|---|---|
| **Request Models & Attributes** | `[Request]`, `[PathOnly]`, `[QueryArg]`, `[FormData]`, `[Header]`, `[Cookie]` and more | [Attributes](https://github.com/gustavorviana/JanusRequest/wiki/Attributes) / [Request Models](https://github.com/gustavorviana/JanusRequest/wiki/Request-Models) |
| **All HTTP Verbs** | GET, POST, PUT, DELETE, PATCH + custom verbs, sync and async | [HttpApiClient Reference](https://github.com/gustavorviana/JanusRequest/wiki/HttpApiClient-&-IHttpApiClient-Reference) |
| **Authentication** | Bearer, Basic, API Key, custom schemes, and `IHttpAuthenticator` | [Authentication](https://github.com/gustavorviana/JanusRequest/wiki/Authentication) / [IHttpAuthenticator](https://github.com/gustavorviana/JanusRequest/wiki/IHttpAuthenticator) |
| **Error Handling** | `HttpErrorHandler`, `ThrottleRecoveryHandler`, `RequestException` | [Error Handling](https://github.com/gustavorviana/JanusRequest/wiki/ErrorHandling) |
| **Custom Deserializers** | `IResponseDeserializer<T>`, `[ResponseDeserializer]` attribute | [Response Handlers](https://github.com/gustavorviana/JanusRequest/wiki/Response-Handlers) |
| **Logging** | `IHttpApiClientLogger` with framework-agnostic lifecycle hooks | [Logging](https://github.com/gustavorviana/JanusRequest/wiki/Logging) |
| **JSON Serialization** | `System.Text.Json` by default, Newtonsoft.Json opt-in | [Serialization](https://github.com/gustavorviana/JanusRequest/wiki/Serialization) |
| **Dependency Injection** | Default and named clients via `IHttpClientFactory` | [Dependency Injection](https://github.com/gustavorviana/JanusRequest/wiki/Dependency-Injection) |
| **Headers & Cookies** | Per-request headers/cookies via attributes or `HttpRequestInfo` | [Headers](https://github.com/gustavorviana/JanusRequest/wiki/Headers) |
| **Advanced Scenarios** | Non-standard body methods, content types, query builder | [Advanced Examples](https://github.com/gustavorviana/JanusRequest/wiki/Advanced-Examples) |

For full documentation, visit the **[Wiki](https://github.com/gustavorviana/JanusRequest/wiki)**.

---

## License

[MIT](LICENSE)