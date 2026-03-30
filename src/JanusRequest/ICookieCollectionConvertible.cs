using System.Collections.Generic;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Interface that allows a type to be converted into a collection of HTTP cookies.
    /// Implement this interface on types used with <see cref="Attributes.CookieCollectionAttribute"/>
    /// to provide custom conversion logic for generating HTTP cookies from an object.
    /// Each <see cref="Cookie"/> can include Path and Domain for scoping.
    /// </summary>
    public interface ICookieCollectionConvertible
    {
        /// <summary>
        /// Converts the current instance to a collection of HTTP cookies.
        /// </summary>
        /// <returns>An enumerable of <see cref="Cookie"/> objects representing HTTP cookies.</returns>
        IEnumerable<Cookie> ToCookieCollection();
    }
}
