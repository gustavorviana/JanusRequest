using System;
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
    /// Internal content translator for application/xml content type.
    /// This translator uses XmlSerializer to serialize and deserialize objects to/from XML format.
    /// Properties marked with QueryArgAttribute or PathOnlyAttribute are excluded from XML serialization.
    /// </summary>
    internal class XmlContentTranslator : ContentTypeTranslator
    {
        private readonly XmlWriterSettings _writerSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            OmitXmlDeclaration = false
        };

        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override string ContentType { get; } = HttpContentType.Xml;

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

            var overrides = CreateXmlAttributeOverrides(typeof(T));
            var serializer = new XmlSerializer(typeof(T), overrides);
            using (var stringReader = new StringReader(xml))
                return (T)serializer.Deserialize(stringReader);
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
        /// Uses UTF-8 encoding with indentation and includes XML declaration.
        /// </summary>
        /// <param name="content">The object to serialize to XML. Can be null.</param>
        /// <returns>The XML string representation of the object, or null if content is null.</returns>
        public override string Serialize(object content)
        {
            if (content == null)
                return null;

            var overrides = CreateXmlAttributeOverrides(content.GetType());
            var serializer = new XmlSerializer(content.GetType(), overrides);
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, _writerSettings))
            {
                serializer.Serialize(xmlWriter, content);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Creates XML attribute overrides to ignore properties marked with disallowed attributes.
        /// This ensures that properties marked with QueryArgAttribute or PathOnlyAttribute
        /// are not included in XML serialization/deserialization.
        /// </summary>
        /// <param name="type">The type to create overrides for.</param>
        /// <returns>An XmlAttributeOverrides instance configured to ignore properties with disallowed attributes.</returns>
        private XmlAttributeOverrides CreateXmlAttributeOverrides(Type type)
        {
            var overrides = new XmlAttributeOverrides();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
                if (property.GetCustomAttributes().Any(x => DisalowedTypes.Contains(x.GetType())))
                    overrides.Add(type, property.Name, new XmlAttributes
                    {
                        XmlIgnore = true
                    });

            return overrides;
        }
    }
}