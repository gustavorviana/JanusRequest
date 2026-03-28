using System.Xml.Serialization;
using JanusRequest.Attributes;

namespace JanusRequest.Integration.Tests.Models;

public class ItemResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class AuthInfoResponse
{
    public string? Scheme { get; set; }
    public string? Token { get; set; }
    public string? User { get; set; }
    public string? Pass { get; set; }
    public string? ApiKey { get; set; }
}

public class EchoResponse
{
    public Dictionary<string, string> Values { get; set; } = new();
}

public class FileUploadResponse
{
    public string? FileName { get; set; }
    public long Size { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new();
}

public class ErrorResponse
{
    public string? Error { get; set; }
}

[ContentType(HttpContentType.Xml)]
[XmlRoot("XmlItem")]
public class XmlItemResponse
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

public class FormEchoResponse
{
    public Dictionary<string, string> Fields { get; set; } = new();
}

public class Base64JsonResponse
{
    public string? Name { get; set; }
    public byte[]? Image { get; set; }
}
