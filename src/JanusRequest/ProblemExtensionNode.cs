using System.Collections.Generic;

namespace JanusRequest
{
    /// <summary>
    /// Represents an extension value node in a <see cref="ProblemDetails"/> response.
    /// A node can hold a simple value, a collection of child nodes, or both.
    /// </summary>
    public class ProblemExtensionNode
    {
        /// <summary>
        /// The scalar value of this node, or null if the node represents an object.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// The child nodes when this node represents an object, or null for scalar values.
        /// </summary>
        public IReadOnlyDictionary<string, ProblemExtensionNode> Children { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemExtensionNode"/> class with a scalar value.
        /// </summary>
        /// <param name="value">The scalar value.</param>
        public ProblemExtensionNode(object value)
            : this(value, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemExtensionNode"/> class with child nodes.
        /// </summary>
        /// <param name="children">The child nodes representing an object structure.</param>
        public ProblemExtensionNode(IReadOnlyDictionary<string, ProblemExtensionNode> children)
            : this(null, children)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemExtensionNode"/> class with a value and child nodes.
        /// </summary>
        /// <param name="value">The scalar value.</param>
        /// <param name="children">The child nodes representing an object structure.</param>
        public ProblemExtensionNode(object value, IReadOnlyDictionary<string, ProblemExtensionNode> children)
        {
            Value = value;
            Children = children;
        }

        /// <summary>
        /// Gets a value indicating whether this node has any child nodes.
        /// </summary>
        public bool HasChildren => Children != null && Children.Count > 0;
    }
}
