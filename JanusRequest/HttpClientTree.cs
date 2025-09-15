using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JanusRequest
{
    internal interface IGetNode
    {
        HttpClientTree.Node GetNode(string name);
    }

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

            var path = new List<string>();
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
                return $"Método \"{methodName}\" não encontrado no caminho. Segmento inválido na posição {index}.";
            }

            return $"Propriedade \"{name}\" não encontrada no caminho. Segmento inválido na posição {index}.";
        }

        private static bool IsValidMethod(MethodInfo method)
        {
            return !method.ContainsGenericParameters &&
                method.GetParameters().Length == 0 &&
                !method.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase) &&
                method.ReturnType != typeof(void) &&
                (method.DeclaringType != typeof(object) || method.Name == nameof(ToString));
        }

        Node IGetNode.GetNode(string name) => _nodes.Find(x => x.Name == name);

        #region NodeClass
        internal abstract class Node : IGetNode
        {
            private readonly List<Node> _nodes = new List<Node>();
            public abstract Type MemberValueType { get; }
            public abstract string Name { get; }

            public ICollection<Node> Nodes => _nodes;

            public virtual bool IsFunction { get; }

            public virtual Node Add(MemberInfo member)
            {
                var node = Parse(member);
                _nodes.Add(node);
                return node;
            }

            public abstract object GetValue(object owner);

            public virtual Node GetNode(string name)
            {
                return _nodes.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            public static Node Parse(MemberInfo member)
            {
                if (member is PropertyInfo property)
                    return new PropertyNode(property);

                if (member is MethodInfo method)
                    return new MethodNode(method);

                throw new NotSupportedException($"O membro {member.MemberType} não é suportado.");
            }

            public void Map()
            {
                foreach (var prop in MemberValueType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var node = Add(prop);
                    if (!ReflectionUtils.IsNative(node.MemberValueType, false))
                        node.Map();
                }

                foreach (var method in MemberValueType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    if (IsValidMethod(method))
                        Add(method);
            }

            public abstract IEnumerable<NodeValue> GetAllValues(object owner, INodeNamer namer);
        }

        private class PropertyNode : Node
        {
            private readonly PropertyInfo _property;

            public override string Name => _property.Name;

            public override Type MemberValueType => _property.PropertyType;

            public PropertyNode(PropertyInfo property)
            {
                _property = property;
            }

            public override object GetValue(object owner) => _property.GetValue(owner);

            public override IEnumerable<NodeValue> GetAllValues(object owner, INodeNamer namer)
            {
                owner = GetValue(owner);
                if (Nodes.Count == 0)
                {
                    if (namer.CanMap(_property))
                        yield return new NodeValue(_property, _property.PropertyType, namer.GetFullName(namer.GetName(_property)), owner);
                    yield break;
                }

                if (!namer.CanEnter(_property))
                    yield break;

                namer.OnEnter(_property);
                foreach (var value in Nodes.SelectMany(node => node.GetAllValues(owner, namer)))
                    yield return value;
                namer.OnLeave();
            }

            public override string ToString()
            {
                return _property.Name;
            }
        }

        private class MethodNode : Node
        {
            private readonly MethodInfo _method;

            public override string Name { get; }

            public override bool IsFunction => true;

            public override Type MemberValueType => throw new NotSupportedException();

            public MethodNode(MethodInfo method)
            {
                Name = method.Name + "()";
                _method = method;
            }

            public override object GetValue(object owner) => _method.Invoke(owner, null);

            public override Node GetNode(string name)
            {
                throw new NotSupportedException();
            }

            public override Node Add(MemberInfo member)
            {
                throw new NotSupportedException();
            }

            public override IEnumerable<NodeValue> GetAllValues(object owner, INodeNamer namer)
            {
                if (namer.CanMap(_method))
                    yield return new NodeValue(_method, _method.ReturnType, namer.GetFullName(namer.GetName(_method)), GetValue(owner));
            }

            public override string ToString()
            {
                return $"{_method.Name}()";
            }
        }

        public class NodeValue
        {
            public MemberInfo Member { get; }
            public string PathName { get; }
            public object Value { get; }
            public Type Type { get; }

            public NodeValue(MemberInfo member, Type type, string pathName, object value)
            {
                Member = member;
                PathName = pathName;
                Value = value;
                Type = type;
            }

            public override string ToString()
            {
                return PathName;
            }
        }

        public interface INodeNamer
        {
            bool CanEnter(PropertyInfo property);
            void OnEnter(PropertyInfo property);
            void OnLeave();

            string GetName(MemberInfo member);
            bool CanMap(MemberInfo member);

            string GetName();
            string GetFullName(string name);
        }

        public class NodeNamer : INodeNamer
        {
            private readonly List<string> _path = new List<string>();

            public virtual bool CanEnter(PropertyInfo property)
            {
                return true;
            }

            public virtual void OnEnter(PropertyInfo property)
            {
                _path.Add(GetName(property));
            }

            public virtual bool CanMap(MemberInfo member)
            {
                return true;
            }

            public virtual string GetName(MemberInfo member)
            {
                return member.Name;
            }

            public virtual void OnLeave()
            {
                _path.RemoveAt(_path.Count - 1);
            }

            public string GetFullName(string name)
            {
                var path = GetName();
                if (string.IsNullOrEmpty(path))
                    return name;

                return $"{path}.{name}";
            }

            public override string ToString()
            {
                return GetName();
            }

            public string GetName()
            {
                return string.Join(".", _path);
            }
        }

        #endregion
    }
}
