using System;
using System.IO;
using System.Net.Http;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Internal content builder for handling buffer-type objects (byte arrays and streams).
    /// This class converts buffer objects into appropriate HttpContent instances for HTTP requests.
    /// </summary>
    internal class BufferContentBuilder
    {
        /// <summary>
        /// Determines whether this builder can handle the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is a buffer type (byte array or stream), false otherwise.</returns>
        public bool CanWork(Type type) => ReflectionUtils.IsBuffer(type);

        /// <summary>
        /// Converts a buffer object (stream or byte array) to HttpContent.
        /// </summary>
        /// <param name="value">The buffer object to convert. Should be a Stream or byte array.</param>
        /// <returns>
        /// StreamContent if the value is a Stream, ByteArrayContent if the value is a byte array.
        /// Sets ContentLength header if the stream size can be determined.
        /// </returns>
        public HttpContent ToHttpContent(object value)
        {
            if (value is Stream stream)
            {
                var content = new StreamContent(stream);
                if (TryGetStreamSize(stream, out var size))
                    content.Headers.ContentLength = size;
                return content;
            }
            return new ByteArrayContent(value as byte[]);
        }

        /// <summary>
        /// Attempts to get the size of a stream if it supports seeking.
        /// </summary>
        /// <param name="stream">The stream to get the size from.</param>
        /// <param name="size">When this method returns, contains the size of the stream if successful, or 0 if unsuccessful.</param>
        /// <returns>True if the stream size was successfully determined, false otherwise.</returns>
        public bool TryGetStreamSize(Stream stream, out long size)
        {
            size = 0;
            try
            {
                if (stream.CanSeek)
                {
                    size = stream.Length;
                    return true;
                }
            }
            catch (NotSupportedException)
            {
            }
            return false;
        }
    }
}