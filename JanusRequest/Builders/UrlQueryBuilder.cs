using JanusRequest.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace JanusRequest.Builders
{
    /// <summary>
    /// Builder class for constructing URL query strings from objects and key-value pairs.
    /// This class provides a fluent interface to build query parameters for HTTP requests,
    /// with support for object serialization, attribute-based configuration, and URL encoding.
    /// </summary>
    public class UrlQueryBuilder
    {
        private readonly Dictionary<string, string> _items = new Dictionary<string, string>();
        private readonly HttpApiClientSettings _settings;

        /// <summary>
        /// Initializes a new instance of the UrlQueryBuilder class with default settings.
        /// </summary>
        public UrlQueryBuilder() : this(HttpApiClientSettings.Default)
        {

        }

        /// <summary>
        /// Initializes a new instance of the UrlQueryBuilder class with specified settings.
        /// </summary>
        /// <param name="settings">The HTTP API client settings to use for serialization and configuration.</param>
        public UrlQueryBuilder(HttpApiClientSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Creates a new UrlQueryBuilder by merging the current builder with another query builder.
        /// </summary>
        /// <param name="query">The query builder to merge with. Can be null.</param>
        /// <returns>A new UrlQueryBuilder instance containing parameters from both builders.</returns>
        public UrlQueryBuilder Merge(UrlQueryBuilder query)
        {
            var newQuery = new UrlQueryBuilder().AddRange(_items);

            if (query != null)
                newQuery.AddRange(query._items);

            return newQuery;
        }

        /// <summary>
        /// Adds all parameters from another UrlQueryBuilder to the current builder.
        /// </summary>
        /// <param name="query">The query builder containing parameters to add.</param>
        /// <returns>The current UrlQueryBuilder instance for method chaining.</returns>
        public UrlQueryBuilder AddAll(UrlQueryBuilder query)
        {
            return AddRange(query._items);
        }

        /// <summary>
        /// Adds a range of key-value pairs as query parameters.
        /// </summary>
        /// <param name="args">The collection of key-value pairs to add as query parameters.</param>
        /// <returns>The current UrlQueryBuilder instance for method chaining.</returns>
        public UrlQueryBuilder AddRange(IEnumerable<KeyValuePair<string, string>> args)
        {
            foreach (var arg in args)
                Set(arg.Key, arg.Value);

            return this;
        }

        /// <summary>
        /// Sets a query parameter with the specified key and value.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value to be converted to string.</param>
        /// <param name="allowEmptyOrNull">Whether to allow empty or null values. If false, empty/null values will be ignored.</param>
        /// <returns>The current UrlQueryBuilder instance for method chaining.</returns>
        public UrlQueryBuilder Set(string key, object value, bool allowEmptyOrNull = true)
        {
            var strValue = _settings.ContentToString(value);
            if (allowEmptyOrNull || !string.IsNullOrEmpty(strValue))
                _items[key] = strValue;

            return this;
        }

        /// <summary>
        /// Adds query parameters from an object by extracting its properties.
        /// Properties marked with QueryIgnoreAttribute will be ignored, and properties marked with
        /// PathOnlyAttribute will also be excluded from query parameters.
        /// </summary>
        /// <param name="obj">The object to extract query parameters from. Can be null.</param>
        /// <param name="withAttributesOnly">If true, only properties marked with QueryArgAttribute will be included.</param>
        /// <returns>The current UrlQueryBuilder instance for method chaining.</returns>
        public UrlQueryBuilder Add(object obj, bool withAttributesOnly = false)
        {
            if (obj == null || ReflectionUtils.IsNative(obj?.GetType(), true))
                return this;

            var treee = _settings.GetTree(obj.GetType());
            foreach (var nodeValue in treee.GetAllValues(obj, new NodeNamer(withAttributesOnly)))
                Set(nodeValue.PathName, GetValue(nodeValue), false);

            return this;
        }

        /// <summary>
        /// Builds a complete URL by combining URL parts with the query string.
        /// </summary>
        /// <param name="urlParts">The URL parts to join together before appending the query string.</param>
        /// <returns>The complete URL with query parameters appended.</returns>
        public string BuildUrl(params string[] urlParts)
        {
            var builder = new StringBuilder();
            builder.Append(string.Join("/", urlParts.Select(x => x?.Trim('/')).Where(x => !string.IsNullOrEmpty(x))));
            Build(builder);
            return builder.ToString();
        }

        /// <summary>
        /// Returns the query string representation of all parameters.
        /// </summary>
        /// <returns>The query string starting with '?' if parameters exist, empty string otherwise.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            Build(builder);
            return builder.ToString();
        }

        private string GetValue(HttpClientTree.NodeValue nodeValue)
        {
            if (ReflectionUtils.IsNative(nodeValue.Type, false))
                return _settings.ContentToString(nodeValue.Value);

            if (!(nodeValue.Value is IEnumerable enumerable))
                return null;

            bool hasValue = false;
            var builder = new StringBuilder();
            foreach (var item in enumerable)
            {
                if (hasValue)
                    builder.Append(',');

                var strVal = _settings.ContentToString(item);
                if (string.IsNullOrEmpty(strVal))
                    continue;

                builder.Append(strVal);
                hasValue = true;
            }

            return builder.ToString();
        }

        private void Build(StringBuilder builder)
        {
            using (var enumerator = _items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;

                builder.AppendFormat("?{0}={1}", enumerator.Current.Key, HttpUtility.UrlEncode(enumerator.Current.Value));

                while (enumerator.MoveNext())
                    builder.AppendFormat("&{0}={1}", enumerator.Current.Key, HttpUtility.UrlEncode(enumerator.Current.Value));
            }
        }

        /// <summary>
        /// Internal class for handling property naming and filtering when extracting query parameters from objects.
        /// This class determines which properties should be included in the query string and how they should be named.
        /// </summary>
        private class NodeNamer : HttpClientTree.NodeNamer
        {
            private readonly bool _needAttribute;

            /// <summary>
            /// Initializes a new instance of the NodeNamer class.
            /// </summary>
            /// <param name="needAttribute">If true, only properties with QueryArgAttribute will be processed.</param>
            public NodeNamer(bool needAttribute)
            {
                _needAttribute = needAttribute;
            }

            /// <summary>
            /// Determines whether the builder can enter (traverse) the specified property.
            /// </summary>
            /// <param name="property">The property to check.</param>
            /// <returns>True if the property can be traversed, false otherwise.</returns>
            public override bool CanEnter(PropertyInfo property)
            {
                return CanMap(property);
            }

            /// <summary>
            /// Determines whether the specified member should be included in the query parameters.
            /// Properties marked with QueryIgnoreAttribute will be excluded.
            /// </summary>
            /// <param name="member">The member to check.</param>
            /// <returns>True if the member should be included, false otherwise.</returns>
            public override bool CanMap(MemberInfo member)
            {
                if (member.MemberType != MemberTypes.Property || member.GetCustomAttribute<QueryIgnoreAttribute>() != null)
                    return false;

                if (!_needAttribute)
                    return true;

                return member.GetCustomAttribute<QueryArgAttribute>() != null;
            }

            /// <summary>
            /// Gets the name to use for the specified member in the query string.
            /// Uses the name specified in QueryArgAttribute if present, otherwise uses the member name.
            /// </summary>
            /// <param name="member">The member to get the name for.</param>
            /// <returns>The name to use in the query string.</returns>
            public override string GetName(MemberInfo member)
            {
                var attribute = member.GetCustomAttribute<QueryArgAttribute>();

                var name = attribute?.Name;
                if (string.IsNullOrEmpty(name))
                    name = member.Name;

                return name;
            }
        }
    }
}