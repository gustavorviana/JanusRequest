using System;

namespace JanusRequest.Attributes
{
    /// <summary>
    /// Specifies the custom deserializer that should be used to convert
    /// the HTTP response content into the decorated response type.
    /// </summary>
    /// <remarks>
    /// This attribute can be applied to a response class to define the default
    /// mechanism used by the HTTP client to transform the raw response payload
    /// into an instance of the target response object.
    /// <para>
    /// If a request explicitly defines its own deserialization strategy,
    /// that strategy should take precedence over this attribute.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ResponseDeserializerAttribute : Attribute
    {
        /// <summary>
        /// Gets the type responsible for converting the HTTP response
        /// into the decorated response type.
        /// </summary>
        public Type DeserializerType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseDeserializerAttribute"/> class.
        /// </summary>
        /// <param name="deserializerType">
        /// The type that will be used to deserialize the HTTP response.
        /// </param>
        public ResponseDeserializerAttribute(Type deserializerType)
        {
            DeserializerType = deserializerType;
        }
    }
}
