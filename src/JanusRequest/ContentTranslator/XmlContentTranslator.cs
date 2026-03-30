using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Content translator for application/xml content type.
    /// This translator uses XmlSerializer to serialize and deserialize objects to/from XML format.
    /// Properties marked with QueryArgAttribute or PathOnlyAttribute are excluded from XML serialization.
    /// </summary>
    public class XmlContentTranslator : ContentTypeTranslator
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache =
            new ConcurrentDictionary<Type, XmlSerializer>();

        private static readonly XmlReaderSettings _defaultReaderSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        private static readonly XmlWriterSettings _defaultWriterSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false
        };

        /// <summary>
        /// Gets the settings used when reading XML during deserialization.
        /// Defaults to DTD processing disabled and no XML resolver (prevents XXE attacks).
        /// </summary>
        public XmlReaderSettings ReaderSettings { get; }

        /// <summary>
        /// Gets the settings used when writing XML during serialization.
        /// Defaults to UTF-8 encoding, indented output, and XML declaration included.
        /// </summary>
        public XmlWriterSettings WriterSettings { get; }

        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override string ContentType { get; } = HttpContentType.Xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlContentTranslator"/> class with default settings.
        /// DTD processing is disabled by default to prevent XXE attacks.
        /// </summary>
        public XmlContentTranslator() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlContentTranslator"/> class with custom settings.
        /// </summary>
        /// <param name="readerSettings">Custom XML reader settings for deserialization. If null, defaults are used (DTD processing disabled).</param>
        /// <param name="writerSettings">Custom XML writer settings for serialization. If null, defaults are used (UTF-8, indented).</param>
        public XmlContentTranslator(XmlReaderSettings readerSettings, XmlWriterSettings writerSettings = null)
        {
            ReaderSettings = readerSettings ?? _defaultReaderSettings;
            WriterSettings = writerSettings ?? _defaultWriterSettings;
        }

        /// <summary>
        /// Deserializes an XML string to the specified type.
        /// Properties marked with disallowed attributes are ignored during deserialization.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the XML to.</typeparam>
        /// <param name="xml">The XML string to deserialize. Can be null or whitespace.</param>
        /// <returns>An instance of type T created from the XML string, or default(T) if xml is null or whitespace.</returns>
        public override T Deserialize<T>(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return default;

            var serializer = GetOrCreateSerializer(typeof(T));
            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, ReaderSettings))
                return (T)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// Converts an object to StringContent with XML representation for HTTP requests.
        /// Properties marked with disallowed attributes (QueryArgAttribute, PathOnlyAttribute) are excluded from serialization.
        /// </summary>
        /// <param name="content">The object to convert to XML content. Can be null.</param>
        /// <returns>
        /// A StringContent instance containing the XML representation of the object with UTF-8 encoding,
        /// or null if the content is null or serializes to empty/null.
        /// </returns>
        public override HttpContent Parse(object content)
        {
            if (content == null)
                return null;

            var xml = Serialize(content);
            if (!string.IsNullOrEmpty(xml))
                return new StringContent(xml, Encoding.UTF8, "application/xml");
            return null;
        }

        /// <summary>
        /// Serializes an object to its XML string representation.
        /// Properties marked with disallowed attributes are excluded from serialization.
        /// </summary>
        /// <param name="content">The object to serialize to XML. Can be null.</param>
        /// <returns>The XML string representation of the object, or null if content is null.</returns>
        public override string Serialize(object content)
        {
            if (content == null)
                return null;

            var serializer = GetOrCreateSerializer(content.GetType());
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, WriterSettings))
            {
                serializer.Serialize(xmlWriter, content);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Clears the internal XmlSerializer cache. Useful for hot-reload or test isolation scenarios.
        /// </summary>
        public static void ClearSerializerCache()
        {
            _serializerCache.Clear();
        }

        private static XmlSerializer GetOrCreateSerializer(Type type)
        {
            return _serializerCache.GetOrAdd(type, t =>
                new XmlSerializer(t, CreateXmlAttributeOverrides(t)));
        }

        private static XmlAttributeOverrides CreateXmlAttributeOverrides(Type type)
        {
            var overrides = new XmlAttributeOverrides();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
                if (property.GetCustomAttributes().Any(x => DisallowedTypes.Contains(x.GetType())))
                    overrides.Add(type, property.Name, new XmlAttributes
                    {
                        XmlIgnore = true
                    });

            return overrides;
        }
    }
}
