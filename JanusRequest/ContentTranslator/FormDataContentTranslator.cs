using System.IO;
using System.Linq;
using System.Net.Http;

namespace JanusRequest.ContentTranslator
{
    /// <summary>
    /// Content translator for multipart/form-data content type.
    /// This translator converts objects to MultipartFormDataContent, handling different property types
    /// including streams, byte arrays, and regular values. Properties marked with QueryArgAttribute
    /// or PathOnlyAttribute are excluded from the form data.
    /// </summary>
    public class FormDataContentTranslator : FormBaseContentTranslator
    {
        /// <summary>
        /// Gets the HTTP content type handled by this translator.
        /// </summary>
        public override HttpContentType ContentType => HttpContentType.FormData;

        /// <summary>
        /// Converts an object to MultipartFormDataContent for HTTP requests.
        /// Each property of the object becomes a form field, with special handling for streams and byte arrays.
        /// Properties marked with disallowed attributes (QueryArgAttribute, PathOnlyAttribute) are ignored.
        /// </summary>
        /// <param name="content">The object to convert to form data. Can be null.</param>
        /// <returns>
        /// A MultipartFormDataContent instance containing the object's properties as form fields,
        /// or null if the content parameter is null.
        /// </returns>
        public override HttpContent Parse(object content)
        {
            if (content == null)
                return null;

            var formData = new MultipartFormDataContent();
            var properties = content.GetType().GetProperties()
                .Where(p => !ShouldIgnoreProperty(p));

            foreach (var property in properties)
            {
                var value = property.GetValue(content);
                if (value == null)
                    continue;

                var propertyName = GetPropertyName(property);

                if (value is Stream stream)
                {
                    formData.Add(new StreamContent(stream), propertyName, propertyName);
                }
                else if (value is byte[] bytes)
                {
                    var byteContent = new ByteArrayContent(bytes);
                    formData.Add(byteContent, propertyName, propertyName);
                }
                else
                {
                    formData.Add(new StringContent(value.ToString()), propertyName);
                }
            }

            return formData;
        }
    }
}