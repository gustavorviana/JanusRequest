using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to opt-out of automatic base64 encoding for byte[] and Stream properties
    /// during JSON serialization. By default, byte[] and Stream properties are serialized as
    /// base64 strings. Applying this attribute preserves the default serializer behavior
    /// (e.g., byte[] as a JSON number array in System.Text.Json).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RawBytesAttribute : Attribute
    {
    }
}
