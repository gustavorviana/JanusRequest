using JanusRequest.ContentTranslator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            ContractResolver = new IgnoreRestApiAttributesContractResolver()
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
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (member.CustomAttributes.Any(x => DisalowedTypes.Contains(x.AttributeType)))
                    property.ShouldSerialize = _ => false;
                return property;
            }
        }
    }
}