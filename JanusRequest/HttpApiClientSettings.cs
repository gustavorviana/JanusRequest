using JanusRequest.Attributes;
using JanusRequest.ContentTranslator;
using JanusRequest.HttpHandlers;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;

namespace JanusRequest
{
    /// <summary>
    /// Configuration settings for HttpApiClient that control content serialization, deserialization,
    /// format providers, and HTTP response handlers. This class manages content type translators,
    /// provides formatting for various data types, and handles response processing strategies.
    /// </summary>
    public class HttpApiClientSettings
    {
        private readonly MediaTypeMap<ContentTypeTranslator> _contentTypeTranslator = new MediaTypeMap<ContentTypeTranslator>();
        private readonly ConcurrentDictionary<Type, HttpClientTree> _httpClientTree = new ConcurrentDictionary<Type, HttpClientTree>();
        private static readonly BufferContentBuilder _bufferReader = new BufferContentBuilder();
        private IFormatProvider _formatProvider = CultureInfo.InvariantCulture;
        private static HttpApiClientSettings _default = new HttpApiClientSettings();
        private IHttpHandlerBase[] _handlers = new IHttpHandlerBase[0];

        /// <summary>
        /// Global content translator overrides keyed by content type name.
        /// When a translator is registered here, it will replace the default
        /// translator for that content type in all future settings instances.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Func<ContentTypeTranslator>> GlobalContentTranslators =
            new ConcurrentDictionary<string, Func<ContentTypeTranslator>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the format string used for DateTime serialization.
        /// Default value is "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Gets or sets the format string used for TimeSpan serialization.
        /// Default value is "HH:mm:ss".
        /// </summary>
        public string TimeFormat { get; set; } = "HH:mm:ss";

        /// <summary>
        /// Gets or sets the format provider used for converting values to strings.
        /// Default value is CultureInfo.InvariantCulture.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when attempting to set a null value.</exception>
        public IFormatProvider FormatProvider
        {
            get => _formatProvider;
            set => _formatProvider = value ?? throw new ArgumentNullException(nameof(FormatProvider));
        }

        /// <summary>
        /// Gets or sets the default settings instance used by HttpApiClient when no specific settings are provided.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when attempting to set a null value.</exception>
        public static HttpApiClientSettings Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(Default));
        }

        private string _defaultMediaType = HttpContentType.Json;

        /// <summary>
        /// Gets or sets the default content type used when no specific content type is specified.
        /// This property is kept for backward compatibility and simply proxies
        /// to <see cref="DefaultMediaType"/>.
        /// </summary>
        /// <remarks>
        /// This property is obsolete. Use <see cref="DefaultMediaType"/> instead.
        /// </remarks>
        [Obsolete("Use " + nameof(DefaultMediaType) + " instead.")]
        public string DefaultContentType
        {
            get => DefaultMediaType;
            set => DefaultMediaType = value;
        }

        /// <summary>
        /// Gets or sets the default HTTP media type used when sending request content
        /// when no specific content type is explicitly provided.
        /// </summary>
        /// <remarks>
        /// The value cannot be null or empty and should contain a valid media type,
        /// such as "application/json".
        /// </remarks>
        public string DefaultMediaType
        {
            get => _defaultMediaType;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException(
                        $"{nameof(DefaultMediaType)} cannot be null or empty.");

                _defaultMediaType = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to validate request models using DataAnnotations before sending.
        /// Default value is true to maintain backward compatibility.
        /// </summary>
        public bool ValidateRequest { get; set; } = true;

        /// <summary>
        /// Gets or sets whether response headers should be included in error logs
        /// produced by <see cref="IHttpApiClientLogger"/> implementations.
        /// Default is false to avoid noisy logs and potential sensitive data exposure.
        /// </summary>
        public bool LogResponseHeadersOnError { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the HttpApiClientSettings class with default content translators.
        /// Sets up JSON, XML, form data, and form URL-encoded content translators.
        /// </summary>
        public HttpApiClientSettings()
        {
            SetContentBuilder(
                new JsonContentTranslator(),
                new XmlContentTranslator(),
                new FormDataContentTranslator(),
                new FormUrlEncodedContentTranslator()
            );
        }

        /// <summary>
        /// Sets the HTTP response handlers used for processing responses and handling errors or recovery.
        /// </summary>
        /// <param name="handlers">Array of handlers implementing IHttpHandlerBase interface.</param>
        /// <returns>The current HttpApiClientSettings instance for method chaining.</returns>
        public HttpApiClientSettings SetHandlers(params IHttpHandlerBase[] handlers)
        {
            _handlers = handlers;
            return this;
        }

        /// <summary>
        /// Sets the content type translators used for serializing and deserializing different content types.
        /// </summary>
        /// <param name="builders">Array of content type translators to register.</param>
        /// <returns>The current HttpApiClientSettings instance for method chaining.</returns>
        public HttpApiClientSettings SetContentBuilder(params ContentTypeTranslator[] builders)
        {
            foreach (var builder in builders)
                _contentTypeTranslator[builder.ContentType] = builder;

            return this;
        }

        /// <summary>
        /// Registers a global content translator override for the specified content type
        /// (for example, "application/json").
        /// This affects all future <see cref="HttpApiClientSettings"/> instances created after
        /// the registration. If <paramref name="factory"/> is null, the override is removed.
        /// </summary>
        /// <param name="contentType">The content type value, e.g. "application/json".</param>
        /// <param name="factory">Factory that creates the translator instance, or null to remove.</param>
        public static void RegisterGlobalContentTranslator(string contentType, Func<ContentTypeTranslator> factory)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type cannot be null or empty.", nameof(contentType));

            contentType = contentType.Trim().ToLowerInvariant();

            if (factory == null)
            {
                GlobalContentTranslators.TryRemove(contentType, out _);
                return;
            }

            GlobalContentTranslators[contentType] = factory;
        }

        /// <summary>
        /// Deserializes a string content to the specified response type using the appropriate content translator.
        /// Uses the content type specified in the type's ContentTypeAttribute, or the provided default, or the DefaultContentType.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize the content to.</typeparam>
        /// <param name="content">The string content to deserialize.</param>
        /// <param name="defaultMediaType">The default content type to use if none is specified. Can be null.</param>
        /// <returns>An instance of TResponse created from the content string.</returns>
        /// <exception cref="NotSupportedException">Thrown when no translator is found for the content type.</exception>
        public TResponse Deserialize<TResponse>(string content, string defaultMediaType = null)
        {
            var type = GetMediaType(typeof(TResponse)) ?? defaultMediaType ?? DefaultMediaType;
            if (!_contentTypeTranslator.TryGetValue(type, out var translator))
                throw new NotSupportedException($"Type {type} does not have a defined converter translator.");

            return translator.Deserialize<TResponse>(content);
        }

        /// <summary>
        /// Attempts to parse an object into HttpContent using the appropriate content translator.
        /// Handles buffer types (streams, byte arrays) specially, and uses content type translators for other types.
        /// </summary>
        /// <param name="contentType">The content type to use for parsing. If null, will attempt to detect from object type.</param>
        /// <param name="request">The object to parse into HttpContent.</param>
        /// <param name="content">When this method returns, contains the HttpContent if successful, null otherwise.</param>
        /// <returns>True if the object was successfully parsed into HttpContent, false otherwise.</returns>
        public bool TryParseContent(string contentType, object request, out HttpContent content)
        {
            if (contentType == null)
            {
                var type = request.GetType();

                if (_bufferReader.CanWork(type))
                {
                    content = _bufferReader.ToHttpContent(request);
                    return true;
                }
            }

            if (_contentTypeTranslator.TryGetValue(contentType ?? DefaultContentType, out var contentBuilder))
            {
                content = contentBuilder.Parse(request);
                return true;
            }

            content = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a handler of the specified type that can handle the given HTTP response.
        /// </summary>
        /// <typeparam name="T">The type of handler to search for.</typeparam>
        /// <param name="response">The HTTP response to find a handler for.</param>
        /// <param name="handler">When this method returns, contains the handler if found, null otherwise.</param>
        /// <returns>True if a suitable handler was found, false otherwise.</returns>
        public bool TryGetHandler<T>(HttpResponseMessage response, out T handler) where T : IHttpHandlerBase
        {
            handler = _handlers.OfType<T>().FirstOrDefault(x => x.CanHandle(response));
            return handler != null;
        }

        /// <summary>
        /// Converts an object value to its string representation using the configured format providers and formats.
        /// Provides special handling for DateTime, DateTimeOffset, TimeSpan, and IConvertible types.
        /// </summary>
        /// <param name="value">The value to convert to string. Can be null.</param>
        /// <returns>
        /// The string representation of the value, or null if the value is null.
        /// Uses custom formatting for date/time types and the configured FormatProvider for other convertible types.
        /// </returns>
        public string ContentToString(object value)
        {
            if (value == null)
                return null;

            if (value is DateTimeOffset dateTimeOffset)
                return dateTimeOffset.ToString(FormatProvider);

            if (value is DateTime dateTime)
                return dateTime.ToString(DateTimeFormat);

            if (value is TimeSpan time)
                return time.ToString(TimeFormat);

            if (value is IConvertible convertible)
                return convertible.ToString(_formatProvider);

            return value.ToString();
        }

        /// <summary>
        /// Gets the media type specified by <see cref="ContentTypeAttribute"/> on the given type.
        /// </summary>
        /// <param name="type">The type to inspect for <see cref="ContentTypeAttribute"/>.</param>
        /// <returns>
        /// The media type string if defined in the attribute; otherwise, null.
        /// </returns>
        internal static string GetMediaType(Type type)
        {
            return type.GetCustomAttribute<ContentTypeAttribute>()?.MediaType;
        }

        /// <summary>
        /// Gets or creates an HttpClientTree for the specified type, used for object property traversal and serialization.
        /// </summary>
        /// <param name="type">The type to get the tree for.</param>
        /// <returns>An HttpClientTree instance for the specified type.</returns>
        internal HttpClientTree GetTree(Type type)
        {
            return _httpClientTree.GetOrAdd(type, treeType => new HttpClientTree(treeType));
        }
    }
}