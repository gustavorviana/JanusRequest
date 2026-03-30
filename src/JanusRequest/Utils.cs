using System;
using System.Collections.Generic;
using System.Net.Http;

namespace JanusRequest
{
    internal static class Utils
    {
        public static IReadOnlyDictionary<string, IReadOnlyList<string>> ExtractHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var header in response.Headers)
                headers[header.Key] = new List<string>(header.Value);

            if (response.Content?.Headers != null)
            {
                foreach (var header in response.Content.Headers)
                    headers[header.Key] = new List<string>(header.Value);
            }

            return headers;
        }
    }
}
