using System;
using System.IO;
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JanusRequest.ContentTranslator.Converters
{
    internal sealed class Base64StreamConverter : JsonConverter<Stream>
    {
        public override Stream Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var base64 = reader.GetString();
            var bytes = Convert.FromBase64String(base64);
            return new MemoryStream(bytes);
        }

        public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            byte[] bytes;
            if (value is MemoryStream ms && ms.TryGetBuffer(out var buffer))
            {
                bytes = new byte[buffer.Count];
                Buffer.BlockCopy(buffer.Array, buffer.Offset, bytes, 0, buffer.Count);
            }
            else
            {
                using (var memoryStream = new MemoryStream())
                {
                    value.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }

            writer.WriteStringValue(Convert.ToBase64String(bytes));
        }
    }
}
#endif
