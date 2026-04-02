using JanusRequest.Attributes;
using JanusRequest.ContentTranslator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Reflection;
using System.Text;

namespace JanusRequest.Json.Newtonsoft
{
    /// <summary>
    /// JSON content translator that uses Newtonsoft.Json explicitly.
    /// Intended to be used from the JanusRequest.Json.Newtonsoft extension
    /// package to override the default System.Text.Json behavior on .NET 5+.
    /// </summary>
    public class NewtonsoftJsonContentTranslator : ContentTypeTranslator
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreRestApiAttributesContractResolver(),
            Converters = { new ProblemDetailsNewtonsoftJsonConverter() }
        };

        public override string ContentType => HttpContentType.Json;

        public override HttpContent Parse(object content)
        {
            var json = Serialize(content);
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;

            return new StringContent(json, Encoding.UTF8, HttpApiClient.JsonContentType);
        }

        public override string Serialize(object content)
        {
            return JsonConvert.SerializeObject(content, _settings);
        }

        public override TResponse Deserialize<TResponse>(string response)
        {
            if (response == null)
                return default;

            return JsonConvert.DeserializeObject<TResponse>(response, _settings);
        }

        private class IgnoreRestApiAttributesContractResolver : DefaultContractResolver
        {
            private static readonly Base64ByteArrayNewtonsoftConverter _byteArrayConverter = new Base64ByteArrayNewtonsoftConverter();
            private static readonly Base64StreamNewtonsoftConverter _streamConverter = new Base64StreamNewtonsoftConverter();

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (member.CustomAttributes.Any(x => DisallowedTypes.Contains(x.AttributeType)))
                {
                    property.ShouldSerialize = _ => false;
                    return property;
                }

                if (member.CustomAttributes.Any(a => a.AttributeType == typeof(RawBytesAttribute)))
                    return property;

                if (property.PropertyType == typeof(byte[]))
                    property.Converter = _byteArrayConverter;
                else if (typeof(Stream).IsAssignableFrom(property.PropertyType))
                    property.Converter = _streamConverter;

                return property;
            }
        }

        private class Base64ByteArrayNewtonsoftConverter : JsonConverter<byte[]>
        {
            public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var base64 = (string)reader.Value;
                return Convert.FromBase64String(base64);
            }

            public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                writer.WriteValue(Convert.ToBase64String(value));
            }
        }

        private class Base64StreamNewtonsoftConverter : JsonConverter<Stream>
        {
            public override Stream ReadJson(JsonReader reader, Type objectType, Stream existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return null;

                var base64 = (string)reader.Value;
                return new MemoryStream(Convert.FromBase64String(base64));
            }

            public override void WriteJson(JsonWriter writer, Stream value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                byte[] bytes;
                if (value is MemoryStream ms)
                {
                    bytes = ms.ToArray();
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        value.CopyTo(memoryStream);
                        bytes = memoryStream.ToArray();
                    }
                }

                writer.WriteValue(Convert.ToBase64String(bytes));
            }
        }
    }
}