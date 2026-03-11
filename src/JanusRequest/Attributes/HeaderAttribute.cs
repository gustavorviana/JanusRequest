using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a property value should be sent as an HTTP header.
    /// The property value is converted to string via <see cref="object.ToString()"/> and used as the header value.
    /// Properties marked with this attribute are excluded from the request body during serialization.
    /// If the property value is null, the header is not sent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderAttribute : Attribute
    {
        /// <summary>
        /// Gets the HTTP header name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderAttribute"/> class.
        /// </summary>
        /// <param name="name">The HTTP header name (e.g., "X-Custom-Header", "Authorization").</param>
        public HeaderAttribute(string name)
        {
            Name = name;
        }
    }
}
