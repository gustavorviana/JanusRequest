using JanusRequest.Json.Newtonsoft;

namespace JanusRequest
{
    /// <summary>
    /// Extension methods to configure <see cref="HttpApiClientSettings"/> to use
    /// Newtonsoft.Json as the JSON serializer on .NET 5.0 and higher.
    /// </summary>
    public static class HttpApiClientSettingsNewtonsoftJsonExtensions
    {
        /// <summary>
        /// Configures the provided <see cref="HttpApiClientSettings"/> instance to use
        /// Newtonsoft.Json for <see cref="HttpContentType.Json"/> serialization on .NET 5+.
        /// It does this by replacing only the JSON translator while keeping the other
        /// content translators (XML, form data, etc.) unchanged.
        /// </summary>
        /// <param name="settings">The settings instance to configure.</param>
        /// <returns>The same <see cref="HttpApiClientSettings"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when settings is null.</exception>
        public static HttpApiClientSettings UseNewtonsoftJson(this HttpApiClientSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            // On .NET 5+ the base JsonContentTranslator already uses System.Text.Json.
            // Here we re-register a Newtonsoft-based translator to override it.
            settings.SetContentBuilder(new NewtonsoftJsonContentTranslator());
            return settings;
        }
    }
}

