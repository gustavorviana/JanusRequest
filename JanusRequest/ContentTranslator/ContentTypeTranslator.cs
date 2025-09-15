using JanusRequest.Attributes;
using System;
using System.Net.Http;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Abstract base class for content type translators that handle serialization and deserialization
    /// of HTTP request and response content. Each translator is responsible for a specific content type
    /// and provides methods to convert objects to HttpContent and parse response strings.
    /// </summary>
    public abstract class ContentTypeTranslator
    {
        /// <summary>
        /// Gets the array of attribute types that should be disallowed or ignored during content processing.
        /// These attributes (QueryArgAttribute, PathOnlyAttribute) indicate that properties should not
        /// be included in the request body content.
        /// </summary>
        protected static Type[] DisalowedTypes { get; } = new[]
        {
            typeof(QueryArgAttribute),
            typeof(PathOnlyAttribute)
        };

        /// <summary>
        /// Gets the HTTP content type that this translator handles.
        /// </summary>
        public abstract HttpContentType ContentType { get; }

        /// <summary>
        /// Converts an object to HttpContent for use in HTTP requests.
        /// </summary>
        /// <param name="content">The object to convert to HttpContent.</param>
        /// <returns>An HttpContent instance representing the serialized object.</returns>
        public abstract HttpContent Parse(object content);

        /// <summary>
        /// Serializes an object to its string representation for the specific content type.
        /// </summary>
        /// <param name="content">The object to serialize.</param>
        /// <returns>The string representation of the object.</returns>
        public abstract string Serialize(object content);

        /// <summary>
        /// Deserializes a response string to the specified type.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize the response to.</typeparam>
        /// <param name="resposne">The response string to deserialize.</param>
        /// <returns>An instance of TResponse created from the response string.</returns>
        public abstract TResponse Deserialize<TResponse>(string resposne);
    }
}