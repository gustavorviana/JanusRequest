using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a property should be ignored during query parameter construction.
    /// This attribute can only be applied to properties and marks them to be excluded 
    /// from the query string when building the request URL. Properties marked with this attribute
    /// will be ignored by the query builder and not included as query parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryIgnoreAttribute : Attribute
    {
    }
}