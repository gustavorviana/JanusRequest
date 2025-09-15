using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to specify the HTTP content type for a class.
    /// This attribute can only be applied to classes and defines which content type
    /// should be used when processing HTTP requests or responses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContentTypeAttribute : Attribute
    {
        public HttpContentType ContentType { get; }

        public ContentTypeAttribute(HttpContentType contentType)
        {
            ContentType = contentType;
        }
    }
}