using JanusRequest.Attributes;
using System;
using System.Linq;
using System.Reflection;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Abstract base class for form-based content translators (such as form-urlencoded and multipart/form-data).
    /// This class provides common functionality for handling form data serialization and property filtering
    /// based on attributes. Form-based translators do not support string serialization/deserialization.
    /// </summary>
    public abstract class FormBaseContentTranslator : ContentTypeTranslator
    {
        /// <summary>
        /// String serialization is not supported for form-based content types.
        /// </summary>
        /// <param name="content">The content to serialize.</param>
        /// <returns>This method always throws NotSupportedException.</returns>
        /// <exception cref="NotSupportedException">Always thrown as form content types should use Parse method instead.</exception>
        public override string Serialize(object content)
        {
            throw new NotSupportedException($"{ContentType} serialization to string is not supported. Use Parse method instead.");
        }

        /// <summary>
        /// String deserialization is not supported for form-based content types.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="content">The content string to deserialize.</param>
        /// <returns>This method always throws NotSupportedException.</returns>
        /// <exception cref="NotSupportedException">Always thrown as form content types do not support deserialization from string.</exception>
        public override T Deserialize<T>(string content)
        {
            throw new NotSupportedException($"{ContentType} deserialization from string is not supported.");
        }

        /// <summary>
        /// Determines whether a property should be ignored during form data processing.
        /// Properties marked with QueryArgAttribute or PathOnlyAttribute will be ignored
        /// as they should not be included in the form data.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns>True if the property should be ignored, false if it should be included in form data.</returns>
        protected virtual internal bool ShouldIgnoreProperty(PropertyInfo property)
        {
            return property.CustomAttributes.Any(x => DisalowedTypes.Contains(x.AttributeType));
        }

        /// <summary>
        /// Gets the name to use for a property in the form data.
        /// Uses the name specified in FormDataAttribute if present, otherwise uses the property name.
        /// </summary>
        /// <param name="property">The property to get the name for.</param>
        /// <returns>The name to use for the property in the form data.</returns>
        protected virtual internal string GetPropertyName(PropertyInfo property)
        {
            return property.GetCustomAttribute<FormDataAttribute>()?.Name ?? property.Name;
        }
    }
}