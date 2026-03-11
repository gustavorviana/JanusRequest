namespace JanusRequest
{
    internal static class MediaTypeNormalizer
    {
        public static string NormalizeMediaType(string key)
        {
            key = key.Trim();

            // Strip parameters if someone passes "application/json; charset=utf-8" as a key by mistake
            var semicolon = key.IndexOf(';');
            if (semicolon >= 0)
                key = key.Substring(0, semicolon);

            return key.Trim();
        }

        public static string GetStructuredSuffixMediaType(string normalizedMediaType)
        {
            // normalizedMediaType = "type/subtype" (lower, no params)
            var slash = normalizedMediaType.IndexOf('/');
            if (slash < 0 || slash == normalizedMediaType.Length - 1)
                return string.Empty;

            var type = normalizedMediaType.Substring(0, slash); // "application"
            var subtype = normalizedMediaType.Substring(slash + 1); // "error+json"

            var plus = subtype.LastIndexOf('+');
            if (plus < 0 || plus == subtype.Length - 1)
                return string.Empty;

            var suffix = subtype.Substring(plus + 1); // "json"
            return type + "/" + suffix; // "application/json"
        }
    }
}
