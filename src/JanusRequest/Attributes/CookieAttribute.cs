using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a property value should be sent as an HTTP cookie.
    /// The property value is converted to string via <see cref="object.ToString()"/> and used as the cookie value.
    /// Properties marked with this attribute are excluded from the request body during serialization.
    /// If the property value is null, the cookie is not sent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CookieAttribute : Attribute
    {
        /// <summary>
        /// Gets the cookie name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the cookie path. When set, the cookie is scoped to this path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the cookie domain. When set, the cookie is scoped to this domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieAttribute"/> class.
        /// </summary>
        /// <param name="name">The cookie name (e.g., "session", "auth_token").</param>
        public CookieAttribute(string name)
        {
            Name = name;
        }
    }
}
