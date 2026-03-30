using JanusRequest.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JanusRequest
{
    internal class HttpClientTree : IGetNode
    {
        private readonly List<Node> _nodes = new List<Node>();
        public Type Type { get; }

        public HttpClientTree(Type type)
        {
            Type = type;
            Map();
        }

        private void Map()
        {
            foreach (var prop in Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var node = Add(prop);
                if (!ReflectionUtils.IsNative(node.MemberValueType, false) && !typeof(IEnumerable).IsAssignableFrom(node.MemberValueType))
                    node.Map();
            }

            foreach (var method in Type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (IsValidMethod(method))
                    Add(method);
        }

        private Node Add(MemberInfo member)
        {
            var node = Node.Parse(member);
            _nodes.Add(node);
            return node;
        }

        public IEnumerable<NodeValue> GetAllValues(object owner, INodeNamer namer = null)
        {
            if (namer == null)
                namer = new NodeNamer();

            return _nodes.SelectMany(node => node.GetAllValues(owner, namer));
        }

        public object GetValue(object owner, string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            var pathParts = path.Split('.');
            if (pathParts.Count(x => x.Contains("()")) > 1)
                throw new ArgumentException("Multiple method calls in the same path are not allowed.");

            IGetNode node = this;
            for (int i = 0; i < pathParts.Length; i++)
            {
                node = node.GetNode(pathParts[i]);
                if (node == null)
                    throw new ArgumentException(GetInvalidMemberMessage(pathParts[i], i));

                owner = ((Node)node).GetValue(owner);
                if (owner == null)
                    return null;
            }

            return owner;
        }

        private static string GetInvalidMemberMessage(string name, int index)
        {
            if (name.EndsWith("()"))
            {
                var methodName = name.Substring(0, name.Length - 2);
                return $"Method \"{methodName}\" not found in path. Invalid segment at position {index}.";
            }

            return $"Property \"{name}\" not found in path. Invalid segment at position {index}.";
        }

        internal static bool IsValidMethod(MethodInfo method)
        {
            return !method.ContainsGenericParameters &&
                method.GetParameters().Length == 0 &&
                !method.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase) &&
                method.ReturnType != typeof(void) &&
                (method.DeclaringType != typeof(object) || method.Name == nameof(ToString));
        }

        Node IGetNode.GetNode(string name) => _nodes.Find(x => x.Name == name);

        #region NodeClass

        #endregion
    }
}
