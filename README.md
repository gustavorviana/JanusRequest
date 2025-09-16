# JanusRequest

A complete HTTP API client library for .NET Framework 4.5+ and .NET 5+ with automatic serialization, error handling, failure recovery, and support for multiple content types. Provides a fluent interface for authentication, headers, and request parameters.

## Features

- **Fluent API Design**: Chain methods for easy configuration
- **Multiple Content Types**: JSON, XML, Form Data, Form URL-Encoded
- **Automatic Serialization/Deserialization**: Based on attributes and content types
- **Error Handling**: Built-in error mapping and recovery mechanisms
- **Authentication Support**: Basic, Bearer, API Key authentication
- **Throttling Management**: Automatic retry with rate limiting support
- **Attribute-Based Configuration**: Configure requests using attributes
- **Sync & Async Support**: Both synchronous and asynchronous operations

## Installation

```bash
Install-Package JanusRequest
```

## Quick Start

### Basic Usage

```csharp
using JanusRequest;

// Create client
var client = new HttpApiClient("https://api.example.com");

// Simple GET request
var response = await client.SendAsync<UserResponse>(new HttpRequestInfo 
{
    Method = "GET",
    Path = "/users/123"
});

Console.WriteLine($"User: {response.Data.Name}");
```

### Using Request Objects with Attributes

```csharp
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
var request = new GetUserRequest { Id = 123, Include = "profile" };
var response = await client.GetAsync(request);
```

### POST Request with JSON Body

```csharp
[Request("/users", Method = "POST")]
[ContentType(HttpContentType.Json)]
public class CreateUserRequest : IRequestResponse<UserResponse>
{
    public string Name { get; set; }
    public string Email { get; set; }
    
    [QueryIgnore] // This won't be included in query parameters
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

### Form Data Upload

```csharp
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

## Authentication

### Bearer Token

```csharp
var client = new HttpApiClient("https://api.example.com")
    .SetBearerAuthentication("your-jwt-token");
```

### Basic Authentication

```csharp
var client = new HttpApiClient("https://api.example.com")
    .SetBasicAuthentication("username", "password");
```

### API Key

```csharp
var client = new HttpApiClient("https://api.example.com")
    .SetApiKeyAuthentication("your-api-key", "X-API-Key");
```

## Error Handling

```csharp
try 
{
    var response = await client.GetAsync(request);
}
catch (RequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.Response}");
    Console.WriteLine($"URL: {ex.Url}");
}
catch (ThrottlingException ex)
{
    Console.WriteLine($"Rate limited. Retry after: {ex.RetryAfter} seconds");
    Console.WriteLine($"Retry at: {ex.RetryAt}");
}
```

## Custom Response Deserializers

```csharp
public class CustomDeserializer : IResponseDeserializer<CustomResponse>
{
    public async Task<CustomResponse> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings)
    {
        var content = await response.Content.ReadAsStringAsync();
        // Custom deserialization logic
        return JsonConvert.DeserializeObject<CustomResponse>(content);
    }
}

[Request("/custom")]
public class CustomRequest : IRequestResponse<CustomResponse, CustomDeserializer>
{
    public string Data { get; set; }
}
```

## Configuration Settings

```csharp
var settings = new HttpApiClientSettings()
    .SetHandlers(new ThrottleRecoveryHandler(), new HttpErrorHandler())
    .SetContentBuilder(new JsonContentTranslator(), new XmlContentTranslator());

settings.DefaultContentType = HttpContentType.Json;
settings.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

var client = new HttpApiClient("https://api.example.com")
{
    Settings = settings
};
```