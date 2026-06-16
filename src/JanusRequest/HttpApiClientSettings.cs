using JanusRequest.Attributes;
using JanusRequest.ContentTranslator;
using JanusRequest.HttpHandlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JanusRequest
{
    /// <summary>
    /// Configuration settings for HttpApiClient that control content serialization, deserialization,
    /// format providers, and HTTP response handlers. This class manages content type translators,
    /// provides formatting for various data types, and handles response processing strategies.
    /// </summary>
    public class HttpApiClientSettings : IDisposable
    {
        private static readonly BufferContentBuilder _bufferReader = new BufferContentBuilder();
        private static HttpApiClientSettings _default = new HttpApiClientSettings();

        private readonly MediaTypeMap<ContentTypeTranslator> _contentTypeTranslator = new MediaTypeMap<ContentTypeTranslator>();
        private readonly ConcurrentDictionary<Type, HttpClientTree> _httpClientTree = new ConcurrentDictionary<Type, HttpClientTree>();
        private readonly ConcurrentDictionary<Type, Type> _deserializerTypeCache = new ConcurrentDictionary<Type, Type>();
        private IFormatProvider _formatProvider = CultureInfo.InvariantCulture;
        private volatile IHttpHandlerBase[] _handlers = new IHttpHandlerBase[0];
        private readonly List<IHttpApiClientLogger> _loggers = new List<IHttpApiClientLogger>();
        private readonly object _loggersLock = new object();
        private readonly object _handlersLock = new object();
        private Type _fallbackDeserializerType;
        private bool _disposed;

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
            get => Volatile.Read(ref _default);
            set => Volatile.Write(ref _default, value ?? throw new ArgumentNullException(nameof(Default)));
        }

        private string _defaultMediaType = HttpContentType.Json;

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
        /// Gets or sets the global authenticator applied to every request made by clients using these settings.
        /// Can be overridden on a per-request basis via <see cref="HttpRequestInfo.Authenticator"/>.
        /// When null, no authenticator-based authentication is applied.
        /// </summary>
        public IHttpAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Gets or sets whether response headers should be included in error logs
        /// produced by <see cref="IHttpApiClientLogger"/> implementations.
        /// Default is false to avoid noisy logs and potential sensitive data exposure.
        /// </summary>
        public bool LogResponseHeadersOnError { get; set; } = false;

        /// <summary>
        /// Gets or sets a custom deserializer for parsing error responses into <see cref="ProblemDetails"/>.
        /// When null (default), the standard JSON deserializer is used.
        /// Set this to handle non-JSON error formats or custom problem details parsing.
        /// </summary>
        public IProblemDeserializer ProblemDeserializer { get; set; }

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
            lock (_handlersLock)
            {
                _handlers = handlers ?? new IHttpHandlerBase[0];
            }
            return this;
        }

        /// <summary>
        /// Appends a handler to the existing handler list. Handlers are evaluated in registration order
        /// (first match wins via <see cref="IHttpHandlerBase.CanHandle"/>), so register specific handlers
        /// before more generic fallbacks.
        /// </summary>
        /// <param name="handler">The handler to append. Cannot be null.</param>
        /// <returns>The current settings instance for fluent chaining.</returns>
        public HttpApiClientSettings AddHandler(IHttpHandlerBase handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_handlersLock)
            {
                var current = _handlers;
                var appended = new IHttpHandlerBase[current.Length + 1];
                Array.Copy(current, appended, current.Length);
                appended[current.Length] = handler;
                _handlers = appended;
            }
            return this;
        }

        /// <summary>
        /// Adds a logger that will be called during request processing.
        /// Multiple loggers can be registered and all will be invoked.
        /// </summary>
        /// <param name="logger">The logger to add.</param>
        /// <returns>The current HttpApiClientSettings instance for method chaining.</returns>
        public HttpApiClientSettings AddLogger(IHttpApiClientLogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            lock (_loggersLock)
            {
                _loggers.Add(logger);
            }
            return this;
        }

        /// <summary>
        /// Gets a snapshot of the registered loggers.
        /// </summary>
        internal IReadOnlyList<IHttpApiClientLogger> Loggers
        {
            get
            {
                lock (_loggersLock)
                {
                    return _loggers.ToArray();
                }
            }
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
        /// Deserializes a string content to the specified response type using the appropriate content translator.
        /// Uses the content type specified in the type's ContentTypeAttribute, or the provided default, or the DefaultMediaType.
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

            if (_contentTypeTranslator.TryGetValue(contentType ?? DefaultMediaType, out var contentBuilder))
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
            var current = _handlers;
            for (var i = 0; i < current.Length; i++)
            {
                if (current[i] is T candidate && candidate.CanHandle(response))
                {
                    handler = candidate;
                    return true;
                }
            }

            handler = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse the response body as <see cref="ProblemDetails"/> using the configured
        /// <see cref="ProblemDeserializer"/> when set, or the JSON content translator otherwise.
        /// Returns null when the body is empty or cannot be parsed as problem details.
        /// </summary>
        /// <param name="response">The HTTP response whose body should be parsed.</param>
        /// <param name="rawResponse">An already-read body string, when available, to avoid re-reading the response content.</param>
        /// <param name="cancellationToken">Cancellation token propagated from the originating request.</param>
        public async Task<ProblemDetails> TryParseProblemDetailsAsync(HttpResponseMessage response, string rawResponse = null, CancellationToken cancellationToken = default)
        {
            if (response?.Content == null)
                return null;

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (ProblemDeserializer != null)
                    return await ProblemDeserializer.DeserializeAsync(response, this);

                var content = rawResponse ?? await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return null;

                return Deserialize<ProblemDetails>(content, HttpContentType.Json);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
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
                return dateTime.ToString(DateTimeFormat, _formatProvider);

            if (value is TimeSpan time)
                return time.ToString(TimeFormat, _formatProvider);

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
        /// Registers a deserializer for the specified response type.
        /// </summary>
        /// <typeparam name="TResponse">The response type that the deserializer can handle.</typeparam>
        /// <typeparam name="TDeserializer">
        /// The deserializer type responsible for converting the HTTP response into <typeparamref name="TResponse"/>.
        /// </typeparam>
        /// <remarks>
        /// This method is a generic convenience wrapper over <see cref="AddDeserializer(Type, Type)"/>.
        /// If a deserializer is already registered for the specified response type, it will be replaced.
        /// </remarks>
        public void AddDeserializer<TResponse, TDeserializer>()
            where TDeserializer : class, IResponseDeserializer<TResponse>, new()
        {
            AddDeserializer(typeof(TResponse), typeof(TDeserializer));
        }

        /// <summary>
        /// Registers or replaces the deserializer associated with the specified response type.
        /// </summary>
        /// <param name="targetClass">The response type that the deserializer should handle.</param>
        /// <param name="deserializer">The type responsible for deserializing the HTTP response.</param>
        /// <remarks>
        /// If a deserializer is already registered for the specified response type, it will be overwritten.
        /// </remarks>
        public void AddDeserializer(Type targetClass, Type deserializer)
        {
            _deserializerTypeCache.AddOrUpdate(targetClass, deserializer, (tClass, tDeserializer) => tDeserializer);
        }

        /// <summary>
        /// Resolves the deserializer type associated with the provided type.
        /// The method first checks whether the type is decorated with
        /// <see cref="ResponseDeserializerAttribute"/> and returns the configured
        /// deserializer if present. If the attribute is not found, it falls back
        /// to inspecting whether the type implements
        /// <see cref="IRequestResponse{TResponse, TDeserializer}"/>.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>
        /// The deserializer <see cref="Type"/> if one can be resolved; otherwise, <see langword="null"/>.
        /// </returns>
        public Type GetDeserializerType(Type type)
        {
            return _deserializerTypeCache.GetOrAdd(type, classType =>
            {
                var attribute = classType.GetCustomAttribute<ResponseDeserializerAttribute>();
                if (attribute != null)
                    return attribute.DeserializerType;

                var interfaceType = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType &&
                                        i.GetGenericTypeDefinition() == typeof(IRequestResponse<,>));

                return interfaceType?.GetGenericArguments()[1];
            });
        }

        /// <summary>
        /// Clears the deserializer type cache. Useful for hot-reload or testing scenarios.
        /// </summary>
        /// <returns>The current HttpApiClientSettings instance for method chaining.</returns>
        public HttpApiClientSettings ClearDeserializerTypeCache()
        {
            _deserializerTypeCache.Clear();
            return this;
        }

        /// <summary>
        /// Sets the fallback open-generic deserializer type used when no specific deserializer
        /// is found for a response type via explicit registration, attribute, or interface.
        /// The type must be an open generic with exactly one type parameter and must implement
        /// <see cref="IResponseDeserializer{TResponse}"/> when closed over a response type.
        /// Pass <see langword="null"/> to clear the fallback.
        /// </summary>
        /// <param name="openGenericType">
        /// An open generic type definition such as <c>typeof(MyDeserializer&lt;&gt;)</c>, or <see langword="null"/> to remove the fallback.
        /// </param>
        /// <returns>The current <see cref="HttpApiClientSettings"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the type is not a generic type definition, does not have exactly one generic type parameter,
        /// or does not implement <see cref="IResponseDeserializer{TResponse}"/>.
        /// </exception>
        public HttpApiClientSettings SetFallbackDeserializer(Type openGenericType)
        {
            if (openGenericType == null)
            {
                _fallbackDeserializerType = null;
                return this;
            }

            if (!openGenericType.IsGenericTypeDefinition)
                throw new ArgumentException(
                    $"Type '{openGenericType.FullName}' must be an open generic type definition (e.g., typeof(MyDeserializer<>)).",
                    nameof(openGenericType));

            if (openGenericType.GetGenericArguments().Length != 1)
                throw new ArgumentException(
                    $"Type '{openGenericType.FullName}' must have exactly one generic type parameter.",
                    nameof(openGenericType));

            var hasDeserializerInterface = openGenericType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IResponseDeserializer<>));

            if (!hasDeserializerInterface)
                throw new ArgumentException(
                    $"Type '{openGenericType.FullName}' does not implement IResponseDeserializer<>.",
                    nameof(openGenericType));

            _fallbackDeserializerType = openGenericType;
            return this;
        }

        /// <summary>
        /// Attempts to resolve a closed deserializer type from the fallback open-generic deserializer
        /// for the specified response type. Returns <see langword="null"/> if no fallback is configured
        /// or if the open generic cannot be closed over the specified type.
        /// </summary>
        /// <param name="responseType">The response type to close the fallback generic over.</param>
        /// <returns>The closed deserializer type, or <see langword="null"/>.</returns>
        internal Type GetFallbackDeserializerType(Type responseType)
        {
            if (_fallbackDeserializerType == null)
                return null;

            // Don't overwrite an explicit registration that was previously cached.
            if (_deserializerTypeCache.TryGetValue(responseType, out var existing) && existing != null)
                return existing;

            try
            {
                var closedType = _fallbackDeserializerType.MakeGenericType(responseType);
                _deserializerTypeCache.TryAdd(responseType, closedType);
                return closedType;
            }
            catch (ArgumentException)
            {
                return null;
            }
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _contentTypeTranslator?.Dispose();
        }
    }
}