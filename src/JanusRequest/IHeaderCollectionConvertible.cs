using System.Collections.Generic;

namespace JanusRequest
{
    /// <summary>
    /// Interface that allows a type to be converted into a collection of HTTP headers.
    /// Implement this interface on types used with <see cref="Attributes.HeaderCollectionAttribute"/>
    /// to provide custom conversion logic for generating HTTP headers from an object.
    /// </summary>
    public interface IHeaderCollectionConvertible
    {
        /// <summary>
        /// Converts the current instance to a collection of HTTP header key-value pairs.
        /// </summary>
        /// <returns>An enumerable of key-value pairs representing HTTP headers.</returns>
        IEnumerable<KeyValuePair<string, string>> ToHeaderCollection();
    }
}
