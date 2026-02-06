using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#endif

namespace JanusRequest.ContentTranslator
{
#if NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER || NET5_0_OR_GREATER
    internal sealed class IgnoreRestApiAttributesResolver : DefaultJsonTypeInfoResolver
    {
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

                if (member.CustomAttributes.Any(a => ContentTypeTranslator.DisalowedTypes.Contains(a.AttributeType)))
                    toRemove.Add(prop);
            }

            foreach (var prop in toRemove)
                typeInfo.Properties.Remove(prop);

            return typeInfo;
        }
    }
#else
    /// <summary>
    /// Custom contract resolver that ignores properties marked with REST API attributes.
    /// This resolver ensures that properties marked with QueryArgAttribute or PathOnlyAttribute
    /// are not included in JSON serialization, as they should be used for URL construction instead.
    /// </summary>
    internal class IgnoreRestApiAttributesContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
    {
        protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.CustomAttributes.Any(x => ContentTypeTranslator.DisalowedTypes.Contains(x.AttributeType)))
                property.ShouldSerialize = _ => false;
            return property;
        }
    }
#endif
}
