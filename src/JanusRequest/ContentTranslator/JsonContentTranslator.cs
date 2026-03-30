using System.Net.Http;
using System.Text;

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#endif

namespace JanusRequest.ContentTranslator
{
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Content translator for application/json content type.
    /// This translator uses System.Text.Json to serialize and deserialize objects to/from JSON format.
    /// Properties marked with QueryArgAttribute or PathOnlyAttribute are excluded from JSON serialization.
    /// </summary>
#else
    /// <summary>
    /// Content translator for application/json content type.
    /// This translator uses Newtonsoft.Json to serialize and deserialize objects to/from JSON format.
    /// Properties marked with QueryArgAttribute or PathOnlyAttribute are excluded from JSON serialization.
    /// </summary>
#endif
    public class JsonContentTranslator : ContentTypeTranslator
    {
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = new IgnoreRestApiAttributesResolver()
        };
#else
        private static readonly Newtonsoft.Json.JsonSerializerSettings _jsonSettings =
            new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new IgnoreRestApiAttributesContractResolver()
            };
#endif
        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override string ContentType => HttpContentType.Json;

        /// <inheritdoc />
        public override HttpContent Parse(object content)
        {
            var json = Serialize(content);
            if (string.IsNullOrEmpty(json) || json == "null")
                return null;

            return new StringContent(json, Encoding.UTF8, HttpApiClient.JsonContentType);
        }

        /// <inheritdoc />
        public override string Serialize(object content)
        {
            if (content == null)
                return "null";

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
            return JsonSerializer.Serialize(content, _jsonOptions);
#else
            return Newtonsoft.Json.JsonConvert.SerializeObject(content, _jsonSettings);
#endif
        }

        /// <inheritdoc />
        public override T Deserialize<T>(string json)
        {
            if (json == null)
                return default;

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _jsonOptions);
#else
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _jsonSettings);
#endif
        }
    }
}