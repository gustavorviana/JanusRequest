using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to specify that a property should be included as a query parameter in the URL.
    /// This attribute can only be applied to properties and marks them to be excluded 
    /// from the request body when sending HTTP requests. Properties marked with this attribute
    /// will be ignored during body serialization and used exclusively as query parameters in the URL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryArgAttribute : Attribute
    {
        public string Name { get; }

        public QueryArgAttribute(string name = null)
        {
            Name = name;
        }
    }
}