using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JanusRequest.Attributes;
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using JanusRequest.ContentTranslator.Converters;
#endif

namespace JanusRequest.ContentTranslator
{
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
    internal sealed class IgnoreRestApiAttributesResolver : DefaultJsonTypeInfoResolver
    {
        private static readonly Base64ByteArrayConverter _byteArrayConverter = new Base64ByteArrayConverter();
        private static readonly Base64StreamConverter _streamConverter = new Base64StreamConverter();

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var typeInfo = base.GetTypeInfo(type, options);

            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return typeInfo;

            var toRemove = new List<JsonPropertyInfo>();

            foreach (var prop in typeInfo.Properties)
            {
                if (!(prop.AttributeProvider is MemberInfo member))
                    continue;

                if (member.CustomAttributes.Any(a => ContentTypeTranslator.DisallowedTypes.Contains(a.AttributeType)))
                {
                    toRemove.Add(prop);
                    continue;
                }

                if (member.CustomAttributes.Any(a => a.AttributeType == typeof(RawBytesAttribute)))
                    continue;

                if (prop.PropertyType == typeof(byte[]))
                    prop.CustomConverter = _byteArrayConverter;
                else if (typeof(Stream).IsAssignableFrom(prop.PropertyType))
                    prop.CustomConverter = _streamConverter;
            }

            foreach (var prop in toRemove)
                typeInfo.Properties.Remove(prop);

            return typeInfo;
        }
    }
#else
    /// <summary>
    /// Custom contract resolver that ignores properties marked with REST API attributes
    /// and applies base64 encoding for byte[] and Stream properties.
    /// </summary>
    internal class IgnoreRestApiAttributesContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.CustomAttributes.Any(x => ContentTypeTranslator.DisallowedTypes.Contains(x.AttributeType)))
            {
                property.ShouldSerialize = _ => false;
                return property;
            }

            if (member.CustomAttributes.Any(a => a.AttributeType == typeof(RawBytesAttribute)))
                return property;

            if (property.PropertyType == typeof(byte[]))
                property.Converter = new Base64ByteArrayNewtonsoftConverter();
            else if (typeof(Stream).IsAssignableFrom(property.PropertyType))
                property.Converter = new Base64StreamNewtonsoftConverter();

            return property;
        }

        private class Base64ByteArrayNewtonsoftConverter : Newtonsoft.Json.JsonConverter<byte[]>
        {
            public override byte[] ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
                    return null;

                var base64 = (string)reader.Value;
                return Convert.FromBase64String(base64);
            }

            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, byte[] value, Newtonsoft.Json.JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                writer.WriteValue(Convert.ToBase64String(value));
            }
        }

        private class Base64StreamNewtonsoftConverter : Newtonsoft.Json.JsonConverter<Stream>
        {
            public override Stream ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, Stream existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
            {
                if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
                    return null;

                var base64 = (string)reader.Value;
                return new MemoryStream(Convert.FromBase64String(base64));
            }

            public override void WriteJson(Newtonsoft.Json.JsonWriter writer, Stream value, Newtonsoft.Json.JsonSerializer serializer)
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
#endif
}
