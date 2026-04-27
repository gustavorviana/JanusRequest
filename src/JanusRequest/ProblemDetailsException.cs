using System.Collections.Generic;
using System.Net;

namespace JanusRequest
{
    /// <summary>
    /// Exception that represents an error described by a <see cref="ProblemDetails"/> response
    /// following RFC 9457 (formerly RFC 7807).
    /// </summary>
    public class ProblemDetailsException : RequestException
    {
        /// <summary>
        /// A URI reference that identifies the problem type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// A short, human-readable summary of the problem type.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// A human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// A URI reference that identifies the specific occurrence of the problem.
        /// </summary>
        public string Instance { get; }

        /// <summary>
        /// Additional members that extend the problem details beyond the standard fields.
        /// </summary>
        public IReadOnlyDictionary<string, ProblemExtensionNode> Extensions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemDetailsException"/> class
        /// from a <see cref="ProblemDetails"/> response and the HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code from the response.</param>
        /// <param name="problem">The problem details response, which may be null or partially filled.</param>
        public ProblemDetailsException(HttpStatusCode statusCode, ProblemDetails problem)
            : base(statusCode, BuildMessage(statusCode, problem))
        {
            Type = problem?.Type;
            Title = problem?.Title;
            Detail = problem?.Detail;
            Instance = problem?.Instance;
            Extensions = problem?.Extensions;
        }

        private static string BuildMessage(HttpStatusCode statusCode, ProblemDetails problem)
        {
            var title = problem?.Title ?? statusCode.ToString();
            var message = $"{title} (Status: {(int)statusCode})";

            if (!string.IsNullOrEmpty(problem?.Detail))
                message = $"{message} -> {problem.Detail}";

            return message;
        }
    }
}
