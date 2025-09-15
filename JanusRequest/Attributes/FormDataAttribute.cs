using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to specify the form data field name for a property.
    /// This attribute can only be applied to properties and defines the name
    /// that should be used when serializing the property as form data in HTTP requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FormDataAttribute : Attribute
    {
        public string Name { get; }

        public FormDataAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }
    }
}