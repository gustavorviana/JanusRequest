using System;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Exception thrown when the HTTP response body cannot be deserialized into the expected type.
    /// Extends <see cref="RequestException"/> to include the raw response content, the target type,
    /// and the original deserialization exception as <see cref="Exception.InnerException"/>.
    /// </summary>
    public class DeserializationException : RequestException
    {
        /// <summary>
        /// Gets the raw response content that failed to deserialize.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the type that the response was being deserialized into.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="content">The raw response content that failed to deserialize.</param>
        /// <param name="targetType">The type that was being deserialized into.</param>
        /// <param name="innerException">The original deserialization exception.</param>
        public DeserializationException(HttpStatusCode statusCode, string content, Type targetType, Exception innerException)
            : base(statusCode, content, innerException)
        {
            Content = content;
            TargetType = targetType;
        }
    }
}
