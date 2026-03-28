using System.Xml.Serialization;
using JanusRequest.Attributes;
using JanusRequest.Integration.Tests.Models;

namespace JanusRequest.Integration.Tests.Models;

// === CRUD ===

[Request("/api/items", Method = "POST")]
public class CreateItemRequest : IRequestResponse<ItemResponse>
{
    public string? Name { get; set; }
}

[Request("/api/items/{Id}", Method = "PUT")]
public class UpdateItemRequest : IRequestResponse<ItemResponse>
{
    [PathOnly]
    public int Id { get; set; }
    public string? Name { get; set; }
}

[Request("/api/items/{Id}", Method = "PATCH")]
public class PatchItemRequest : IRequestResponse<ItemResponse>
{
    [PathOnly]
    public int Id { get; set; }
    public string? Name { get; set; }
}

// === Headers ===

[Request("/api/echo/headers", Method = "POST")]
public class HeaderTestRequest : IRequestResponse<EchoResponse>
{
    [Header("X-Custom-Header")]
    public string? CustomHeader { get; set; }

    [Header("X-Trace-Id")]
    public string? TraceId { get; set; }
}

[Request("/api/echo/headers", Method = "POST")]
public class HeaderCollectionTestRequest : IRequestResponse<EchoResponse>
{
    [HeaderCollection]
    public Dictionary<string, string>? Headers { get; set; }
}

// === Cookies ===

[Request("/api/echo/cookies", Method = "POST")]
public class CookieTestRequest : IRequestResponse<EchoResponse>
{
    [Cookie("session")]
    public string? SessionId { get; set; }

    [Cookie("theme")]
    public string? Theme { get; set; }
}

[Request("/api/echo/cookies", Method = "POST")]
public class CookieCollectionTestRequest : IRequestResponse<EchoResponse>
{
    [CookieCollection]
    public Dictionary<string, string>? Cookies { get; set; }
}

// === Query Parameters ===

[Request("/api/echo/query")]
public class QueryTestRequest : IRequestResponse<EchoResponse>
{
    [QueryArg("page")]
    public int Page { get; set; }

    [QueryArg("size")]
    public int Size { get; set; }

    [QueryArg("search")]
    public string? Search { get; set; }
}

// === File Upload ===

[Request("/api/files/upload", Method = "POST")]
[ContentType(HttpContentType.FormData)]
public class FileUploadByteArrayRequest : IRequestResponse<FileUploadResponse>
{
    [FormData("file")]
    public byte[]? File { get; set; }

    [FormData("description")]
    public string? Description { get; set; }
}

[Request("/api/files/upload", Method = "POST")]
[ContentType(HttpContentType.FormData)]
public class FileUploadStreamRequest : IRequestResponse<FileUploadResponse>
{
    [FormData("file")]
    public Stream? File { get; set; }

    [FormData("description")]
    public string? Description { get; set; }
}

// === XML ===

[Request("/api/content/xml", Method = "POST")]
[ContentType(HttpContentType.Xml)]
[XmlRoot("XmlItem")]
public class XmlItemRequest : IRequestResponse<XmlItemResponse>
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

// === Form URL Encoded ===

[Request("/api/content/form-urlencoded", Method = "POST")]
[ContentType(HttpContentType.FormUrlEncoded)]
public class FormUrlEncodedRequest : IRequestResponse<FormEchoResponse>
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
