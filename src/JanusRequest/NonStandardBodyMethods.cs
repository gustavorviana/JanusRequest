using System;

namespace JanusRequest
{
    /// <summary>
    /// Defines HTTP methods that normally do not support a request body,
    /// but can be explicitly enabled when interacting with APIs that accept it.
    /// </summary>
    [Flags]
    public enum NonStandardBodyMethods
    {
        /// <summary>
        /// No non-standard methods are allowed to send a body.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allows sending a body with GET requests.
        /// Although not forbidden by HTTP, most servers ignore or reject it.
        /// </summary>
        Get = 1 << 0,

        /// <summary>
        /// Allows sending a body with DELETE requests.
        /// Some APIs use it, but support is inconsistent.
        /// </summary>
        Delete = 1 << 1,

        /// <summary>
        /// Allows sending bodies with all non-standard methods.
        /// </summary>
        All = Get | Delete
    }
}
