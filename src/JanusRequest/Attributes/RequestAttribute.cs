using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to define HTTP request configuration for a class.
    /// This attribute can only be applied to classes and specifies the HTTP method 
    /// and URL path that should be used when making requests. The default HTTP method 
    /// is GET if not explicitly specified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequestAttribute : Attribute
    {
        public string Method { get; set; } = "GET";
        public string Path { get; }

        public RequestAttribute(string path)
        {
            Path = path;
        }
    }
}