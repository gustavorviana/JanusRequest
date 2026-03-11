using System;
using System.Collections.Generic;
using System.Text;

namespace JanusRequest.Builders
{
    /// <summary>
    /// Builder class for constructing URLs from templates with parameter placeholders.
    /// This class replaces placeholders in URL templates (e.g., {id}, {name}) with actual values
    /// from parameter objects, providing a flexible way to build dynamic URLs.
    /// </summary>
    public class UrlBuilder
    {
        private readonly string _template;
        private readonly HttpApiClientSettings _settings;

        /// <summary>
        /// Initializes a new instance of the UrlBuilder class with a URL template.
        /// </summary>
        /// <param name="template">The URL template containing placeholders in the format {parameterName}.</param>
        /// <param name="settings">The HTTP API client settings to use. If null, default settings will be used.</param>
        public UrlBuilder(string template, HttpApiClientSettings settings = null)
        {
            _template = template;
            _settings = settings ?? HttpApiClientSettings.Default;
        }

        /// <summary>
        /// Builds a URL by replacing placeholders in the template with values from the parameters object.
        /// </summary>
        /// <param name="parameters">The object containing parameter values to replace placeholders. Cannot be null if template contains placeholders.</param>
        /// <returns>The built URL with all placeholders replaced by their corresponding values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parameters is null and the template contains placeholders.</exception>
        public string Build(object parameters)
        {
            if (string.IsNullOrEmpty(_template))
                return _template;

            var placeholders = ExtractArguments(_template);
            if (placeholders.Count == 0)
                return _template;

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var builder = new StringBuilder(_template);
            var tree = _settings.GetTree(parameters.GetType());

            for (int i = placeholders.Count - 1; i >= 0; i--)
            {
                var placeholder = placeholders[i];
                var value = _settings.ContentToString(tree.GetValue(parameters, placeholder.FullName));
                builder
                    .Remove(placeholder.Index, placeholder.Length + 2)
                    .Insert(placeholder.Index, value ?? "Null");
            }

            return builder.ToString();
        }

        private List<UrlArgument> ExtractArguments(string template)
        {
            var placeholders = new List<UrlArgument>();
            int startIndex = 0;

            while (startIndex < template.Length)
            {
                int openBrace = template.IndexOf('{', startIndex);
                if (openBrace == -1) break;

                int closeBrace = template.IndexOf('}', openBrace);
                if (closeBrace == -1) break;

                string placeholder = template.Substring(openBrace + 1, closeBrace - openBrace - 1);
                placeholders.Add(new UrlArgument(placeholder, openBrace));
                startIndex = closeBrace + 1;
            }

            return placeholders;
        }

        /// <summary>
        /// Represents a URL parameter argument with its name, position, and length in the template.
        /// </summary>
        private readonly struct UrlArgument
        {
            /// <summary>
            /// Gets the full name of the parameter as it appears in the placeholder.
            /// </summary>
            public string FullName { get; }

            /// <summary>
            /// Gets the starting index position of the placeholder in the template.
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// Gets the length of the parameter name.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// Initializes a new instance of the UrlArgument struct.
            /// </summary>
            /// <param name="name">The parameter name.</param>
            /// <param name="startIndex">The starting index of the placeholder in the template.</param>
            public UrlArgument(string name, int startIndex)
            {
                Length = name.Length;
                Index = startIndex;
                FullName = name;
            }

            /// <summary>
            /// Returns the full name of the parameter.
            /// </summary>
            /// <returns>The parameter name as a string.</returns>
            public override string ToString()
            {
                return FullName;
            }
        }
    }
}