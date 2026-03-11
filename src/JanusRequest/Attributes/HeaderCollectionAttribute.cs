using System;
namespace JanusRequest.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a property contains a collection of HTTP headers.
    /// The property value is resolved in the following order:
    /// <list type="number">
    ///   <item><see cref="IHeaderCollectionConvertible"/> - uses <see cref="IHeaderCollectionConvertible.ToHeaderCollection()"/></item>
    ///   <item><see cref="System.Collections.IDictionary"/> - iterates entries converting keys and values via ToString()</item>
    ///   <item><see cref="System.Collections.Generic.IEnumerable{T}"/> of <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/> - iterates pairs</item>
    /// </list>
    /// Properties marked with this attribute are excluded from the request body during serialization.
    /// If the property value is null, no headers are sent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HeaderCollectionAttribute : Attribute
    {
    }
}
