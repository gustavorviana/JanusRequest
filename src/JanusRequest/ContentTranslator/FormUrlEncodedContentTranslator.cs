using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Content translator for application/x-www-form-urlencoded content type.
    /// This translator converts objects to FormUrlEncodedContent by extracting object properties
    /// and creating key-value pairs. Properties marked with QueryArgAttribute or PathOnlyAttribute
    /// are excluded from the form data.
    /// </summary>
    public class FormUrlEncodedContentTranslator : FormBaseContentTranslator
    {
        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override string ContentType => HttpContentType.FormUrlEncoded;

        /// <summary>
        /// Converts an object to FormUrlEncodedContent for HTTP requests.
        /// Each property of the object becomes a key-value pair in the form data.
        /// Properties marked with disallowed attributes (QueryArgAttribute, PathOnlyAttribute) are ignored.
        /// Null property values are skipped.
        /// </summary>
        /// <param name="content">The object to convert to form-urlencoded data. Can be null.</param>
        /// <returns>
        /// A FormUrlEncodedContent instance containing the object's properties as key-value pairs,
        /// or null if the content parameter is null.
        /// </returns>
        public override HttpContent Parse(object content)
        {
            if (content == null)
                return null;

            var keyValuePairs = new List<KeyValuePair<string, string>>();
            var properties = content.GetType().GetProperties()
                .Where(p => !ShouldIgnoreProperty(p));

            foreach (var property in properties)
            {
                var value = property.GetValue(content);
                if (value == null)
                    continue;

                keyValuePairs.Add(new KeyValuePair<string, string>(GetPropertyName(property), value.ToString()));
            }

            return new FormUrlEncodedContent(keyValuePairs);
        }
    }
}