using System;
using System.Collections.Generic;

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JanusRequest.ContentTranslator.Converters
{
    /// <summary>
    /// Converts a JSON response into a <see cref="ProblemDetails"/> instance following RFC 9457.
    /// Known fields (type, title, status, detail, instance) are mapped to properties;
    /// all other fields are placed into <see cref="ProblemDetails.Extensions"/> as <see cref="ProblemExtensionNode"/> trees.
    /// Supports root-level arrays by mapping each element to an index-based extension key ("0", "1", ...).
    /// </summary>
    internal sealed class ProblemDetailsJsonConverter : JsonConverter<ProblemDetails>
    {
        /// <inheritdoc />
        public override ProblemDetails Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
                return ReadFromArray(ref reader);

            if (reader.TokenType != JsonTokenType.StartObject)
                return ReadFromScalar(ref reader);

            return ReadFromObject(ref reader);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, ProblemDetails value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.Type != null)
            {
                writer.WriteString("type", value.Type);
            }

            if (value.Title != null)
            {
                writer.WriteString("title", value.Title);
            }

            writer.WriteNumber("status", value.Status);

            if (value.Detail != null)
            {
                writer.WriteString("detail", value.Detail);
            }

            if (value.Instance != null)
            {
                writer.WriteString("instance", value.Instance);
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

        private static ProblemDetails ReadFromObject(ref Utf8JsonReader reader)
        {
            string type = null;
            string title = null;
            int status = 0;
            string detail = null;
            string instance = null;
            Dictionary<string, ProblemExtensionNode> extensions = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Expected PropertyName token.");

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "type":
                        type = reader.GetString();
                        break;
                    case "title":
                        title = reader.GetString();
                        break;
                    case "status":
                        status = reader.GetInt32();
                        break;
                    case "detail":
                        detail = reader.GetString();
                        break;
                    case "instance":
                        instance = reader.GetString();
                        break;
                    default:
                        if (extensions == null)
                            extensions = new Dictionary<string, ProblemExtensionNode>();

                        extensions[propertyName] = ReadNode(ref reader);
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

        private static ProblemDetails ReadFromArray(ref Utf8JsonReader reader)
        {
            var extensions = new Dictionary<string, ProblemExtensionNode>();
            var index = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                extensions[index.ToString()] = ReadNode(ref reader);
                index++;
            }

            return new ProblemDetails(null, null, 0, extensions: extensions);
        }

        private static ProblemDetails ReadFromScalar(ref Utf8JsonReader reader)
        {
            var node = ReadNode(ref reader);
            var extensions = new Dictionary<string, ProblemExtensionNode> { ["0"] = node };
            return new ProblemDetails(null, null, 0, extensions: extensions);
        }

        private static ProblemExtensionNode ReadNode(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return new ProblemExtensionNode(reader.GetString());

                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var longValue))
                        return new ProblemExtensionNode(longValue);

                    return new ProblemExtensionNode(reader.GetDouble());

                case JsonTokenType.True:
                    return new ProblemExtensionNode(true);

                case JsonTokenType.False:
                    return new ProblemExtensionNode(false);

                case JsonTokenType.Null:
                    return new ProblemExtensionNode((object)null);

                case JsonTokenType.StartObject:
                    return ReadObjectNode(ref reader);

                case JsonTokenType.StartArray:
                    return ReadArrayNode(ref reader);

                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        private static ProblemExtensionNode ReadObjectNode(ref Utf8JsonReader reader)
        {
            var children = new Dictionary<string, ProblemExtensionNode>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                var key = reader.GetString();
                reader.Read();
                children[key] = ReadNode(ref reader);
            }

            return new ProblemExtensionNode(children);
        }

        private static ProblemExtensionNode ReadArrayNode(ref Utf8JsonReader reader)
        {
            var children = new Dictionary<string, ProblemExtensionNode>();
            var index = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                children[index.ToString()] = ReadNode(ref reader);
                index++;
            }

            return new ProblemExtensionNode(children);
        }

        private static void WriteNode(Utf8JsonWriter writer, ProblemExtensionNode node)
        {
            if (node == null)
            {
                writer.WriteNullValue();
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

        private static void WriteValue(Utf8JsonWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case string s:
                    writer.WriteStringValue(s);
                    break;
                case long l:
                    writer.WriteNumberValue(l);
                    break;
                case int i:
                    writer.WriteNumberValue(i);
                    break;
                case double d:
                    writer.WriteNumberValue(d);
                    break;
                case float f:
                    writer.WriteNumberValue(f);
                    break;
                case decimal m:
                    writer.WriteNumberValue(m);
                    break;
                case bool b:
                    writer.WriteBooleanValue(b);
                    break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
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
