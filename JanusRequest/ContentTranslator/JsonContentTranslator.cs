using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Content translator for application/json content type.
    /// This translator uses Newtonsoft.Json to serialize and deserialize objects to/from JSON format.
    /// Properties marked with QueryArgAttribute or PathOnlyAttribute are excluded from JSON serialization.
    /// </summary>
    public class JsonContentTranslator : ContentTypeTranslator
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreRestApiAttributesContractResolver()
        };

        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override HttpContentType ContentType => HttpContentType.Json;

        /// <summary>
        /// Converts an object to StringContent with JSON representation for HTTP requests.
        /// Properties marked with disallowed attributes (QueryArgAttribute, PathOnlyAttribute) are excluded from serialization.
        /// </summary>
        /// <param name="content">The object to convert to JSON content. Can be null.</param>
        /// <returns>
        /// A StringContent instance containing the JSON representation of the object with UTF-8 encoding,
        /// or null if the content is null or serializes to "null".
        /// </returns>
        public override HttpContent Parse(object content)
        {
            var json = JsonConvert.SerializeObject(content, _settings);
            if (!string.IsNullOrEmpty(json) && json != "null")
                return new StringContent(json, Encoding.UTF8, HttpApiClient.JsonContentType);
            return null;
        }

        /// <summary>
        /// Serializes an object to its JSON string representation.
        /// Properties marked with disallowed attributes are excluded from serialization.
        /// </summary>
        /// <param name="content">The object to serialize to JSON.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public override string Serialize(object content)
        {
            return JsonConvert.SerializeObject(content, _settings);
        }

        /// <summary>
        /// Deserializes a JSON string to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the JSON to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An instance of type T created from the JSON string.</returns>
        public override T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        /// <summary>
        /// Custom contract resolver that ignores properties marked with REST API attributes.
        /// This resolver ensures that properties marked with QueryArgAttribute or PathOnlyAttribute
        /// are not included in JSON serialization, as they should be used for URL construction instead.
        /// </summary>
        private class IgnoreRestApiAttributesContractResolver : DefaultContractResolver
        {
            /// <summary>
            /// Creates a JSON property and configures it to be ignored if it has disallowed attributes.
            /// </summary>
            /// <param name="member">The member to create a property for.</param>
            /// <param name="memberSerialization">The member serialization settings.</param>
            /// <returns>A JsonProperty configured to ignore serialization if the member has disallowed attributes.</returns>
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (member.CustomAttributes.Any(x => DisalowedTypes.Contains(x.AttributeType)))
                    property.ShouldSerialize = instance => false;
                return property;
            }
        }
    }
}