using System;
using System.Collections.Generic;

#if !NETSTANDARD2_0_OR_GREATER && !NET472_OR_GREATER && !NET5_0_OR_GREATER
using Newtonsoft.Json;

namespace JanusRequest.ContentTranslator.Converters
{
    /// <summary>
    /// Converts a JSON response into a <see cref="ProblemDetails"/> instance following RFC 9457.
    /// Known fields (type, title, status, detail, instance) are mapped to properties;
    /// all other fields are placed into <see cref="ProblemDetails.Extensions"/> as <see cref="ProblemExtensionNode"/> trees.
    /// Supports root-level arrays by mapping each element to an index-based extension key ("0", "1", ...).
    /// </summary>
    internal sealed class ProblemDetailsNewtonsoftJsonConverter : JsonConverter<ProblemDetails>
    {
        /// <inheritdoc />
        public override ProblemDetails ReadJson(JsonReader reader, Type objectType, ProblemDetails existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
                return ReadFromArray(reader);

            if (reader.TokenType != JsonToken.StartObject)
                return ReadFromScalar(reader);

            return ReadFromObject(reader);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, ProblemDetails value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (value.Type != null)
            {
                writer.WritePropertyName("type");
                writer.WriteValue(value.Type);
            }

            if (value.Title != null)
            {
                writer.WritePropertyName("title");
                writer.WriteValue(value.Title);
            }

            writer.WritePropertyName("status");
            writer.WriteValue(value.Status);

            if (value.Detail != null)
            {
                writer.WritePropertyName("detail");
                writer.WriteValue(value.Detail);
            }

            if (value.Instance != null)
            {
                writer.WritePropertyName("instance");
                writer.WriteValue(value.Instance);
            }

            if (value.Extensions != null)
            {
                foreach (var kvp in value.Extensions)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteNode(writer, kvp.Value);
                }
            }

            writer.WriteEndObject();
        }

        private static ProblemDetails ReadFromObject(JsonReader reader)
        {
            string type = null;
            string title = null;
            int status = 0;
            string detail = null;
            string instance = null;
            Dictionary<string, ProblemExtensionNode> extensions = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    throw new JsonSerializationException("Expected PropertyName token.");

                var propertyName = (string)reader.Value;
                reader.Read();

                switch (propertyName)
                {
                    case "type":
                        type = (string)reader.Value;
                        break;
                    case "title":
                        title = (string)reader.Value;
                        break;
                    case "status":
                        status = Convert.ToInt32(reader.Value);
                        break;
                    case "detail":
                        detail = (string)reader.Value;
                        break;
                    case "instance":
                        instance = (string)reader.Value;
                        break;
                    default:
                        if (extensions == null)
                            extensions = new Dictionary<string, ProblemExtensionNode>();

                        extensions[propertyName] = ReadNode(reader);
                        break;
                }
            }

            return new ProblemDetails(
                type,
                title,
                status,
                detail,
                instance,
                extensions
            );
        }

        private static ProblemDetails ReadFromArray(JsonReader reader)
        {
            var extensions = new Dictionary<string, ProblemExtensionNode>();
            var index = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                extensions[index.ToString()] = ReadNode(reader);
                index++;
            }

            return new ProblemDetails(null, null, 0, extensions: extensions);
        }

        private static ProblemDetails ReadFromScalar(JsonReader reader)
        {
            var node = ReadNode(reader);
            var extensions = new Dictionary<string, ProblemExtensionNode> { ["0"] = node };
            return new ProblemDetails(null, null, 0, extensions: extensions);
        }

        private static ProblemExtensionNode ReadNode(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return new ProblemExtensionNode((string)reader.Value);

                case JsonToken.Integer:
                    return new ProblemExtensionNode((long)reader.Value);

                case JsonToken.Float:
                    return new ProblemExtensionNode((double)reader.Value);

                case JsonToken.Boolean:
                    return new ProblemExtensionNode((bool)reader.Value);

                case JsonToken.Null:
                    return new ProblemExtensionNode((object)null);

                case JsonToken.StartObject:
                    return ReadObjectNode(reader);

                case JsonToken.StartArray:
                    return ReadArrayNode(reader);

                default:
                    throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
            }
        }

        private static ProblemExtensionNode ReadObjectNode(JsonReader reader)
        {
            var children = new Dictionary<string, ProblemExtensionNode>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                var key = (string)reader.Value;
                reader.Read();
                children[key] = ReadNode(reader);
            }

            return new ProblemExtensionNode(children);
        }

        private static ProblemExtensionNode ReadArrayNode(JsonReader reader)
        {
            var children = new Dictionary<string, ProblemExtensionNode>();
            var index = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                children[index.ToString()] = ReadNode(reader);
                index++;
            }

            return new ProblemExtensionNode(children);
        }

        private static void WriteNode(JsonWriter writer, ProblemExtensionNode node)
        {
            if (node == null)
            {
                writer.WriteNull();
                return;
            }

            if (!node.HasChildren)
            {
                WriteValue(writer, node.Value);
                return;
            }

            if (IsArrayLike(node.Children))
            {
                writer.WriteStartArray();

                for (var i = 0; i < node.Children.Count; i++)
                    WriteNode(writer, node.Children[i.ToString()]);

                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStartObject();

                foreach (var kvp in node.Children)
                {
                    writer.WritePropertyName(kvp.Key);
                    WriteNode(writer, kvp.Value);
                }

                writer.WriteEndObject();
            }
        }

        private static void WriteValue(JsonWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(value);
        }

        private static bool IsArrayLike(IReadOnlyDictionary<string, ProblemExtensionNode> children)
        {
            if (children == null || children.Count == 0)
                return false;

            for (var i = 0; i < children.Count; i++)
            {
                if (!children.ContainsKey(i.ToString()))
                    return false;
            }

            return true;
        }
    }
}
#endif
