namespace JanusRequest
{
    /// <summary>
    /// Defines common HTTP content types used in web request/response scenarios.
    /// Values represent the media type used for serialization or special handling.
    /// </summary>
    public static class HttpContentType
    {
        /// <summary>
        /// No specific content type defined. Uses default serialization behavior.
        /// </summary>
        public const string None = "";

        /// <summary>
        /// Represents multipart form data, typically used for file uploads.
        /// Serialized as "multipart/form-data".
        /// </summary>
        public const string FormData = "multipart/form-data";

        /// <summary>
        /// Represents standard form data serialized as
        /// "application/x-www-form-urlencoded".
        /// </summary>
        public const string FormUrlEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Represents query string serialization.
        /// This is not an HTTP Content-Type header value and is internally
        /// represented as "@query".
        /// </summary>
        public const string QueryString = "@query";

        /// <summary>
        /// Represents JSON content serialized as "application/json".
        /// </summary>
        public const string Json = "application/json";

        /// <summary>
        /// Represents XML content serialized as "application/xml".
        /// Some systems may also use "text/xml".
        /// </summary>
        public const string Xml = "application/xml";
    }
}