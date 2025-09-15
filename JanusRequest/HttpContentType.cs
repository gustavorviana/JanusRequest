namespace JanusRequest
{
    /// <summary>
    /// Specifies the purpose and content type of a class in web request/response scenarios.
    /// Used as an attribute parameter to indicate how the class data should be interpreted and serialized.
    /// </summary>
    public enum HttpContentType
    {
        /// <summary>
        /// No specific content type defined. Uses default serialization behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Class represents form data with file uploads or binary content.
        /// Will be serialized as multipart/form-data when used in HTTP requests.
        /// </summary>
        FormData = 1,

        /// <summary>
        /// Class represents standard form data.
        /// Will be serialized as application/x-www-form-urlencoded when used in HTTP requests.
        /// </summary>
        FormUrlEncoded = 2,

        /// <summary>
        /// Class properties represent query string parameters.
        /// Will be serialized as URL parameters when used in HTTP requests.
        /// </summary>
        QueryString = 3,

        /// <summary>
        /// Class represents JSON content data.
        /// Will be serialized as application/json when used in HTTP requests.
        /// </summary>
        /// 
        Json = 4,

        /// <summary>
        /// Class represents XML content data.
        /// Will be serialized as application/xml or text/xml when used in HTTP requests.
        /// </summary>
        Xml = 5
    }
}