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
        /// <summary>
        /// Gets the raw HTTP media type string (for example, "application/json").
        /// This allows specifying custom or vendor-specific content types that are not
        /// covered by <see cref="HttpContentType"/>.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// Initializes the attribute using a raw HTTP media type string
        /// (for example, "application/json").
        /// </summary>
        /// <param name="mediaType">The HTTP media type.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="mediaType"/> is null or empty.
        /// </exception>
        public ContentTypeAttribute(string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
                throw new ArgumentException("Media type cannot be null or empty.", nameof(mediaType));

            MediaType = mediaType.Trim();
        }
    }
}