using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a property should only be used for URL path construction.
    /// This attribute can only be applied to properties and marks them to be excluded 
    /// from the request body when sending HTTP requests. Properties marked with this attribute
    /// will be ignored during body serialization and used exclusively for building the request URL path.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PathOnlyAttribute : Attribute
    {
    }
}