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
            var type = content.GetType();
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new IgnoreRestApiAttributesResolver()
            };

            return JsonSerializer.Serialize(content, options);
#else
            return Newtonsoft.Json.JsonConvert.SerializeObject(
                content,
                new Newtonsoft.Json.JsonSerializerSettings
                {
                    ContractResolver = new IgnoreRestApiAttributesContractResolver()
                });
#endif
        }

        /// <inheritdoc />
        public override T Deserialize<T>(string json)
        {
            if (json == null)
                return default;

#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
#else
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
#endif
        }
    }
}