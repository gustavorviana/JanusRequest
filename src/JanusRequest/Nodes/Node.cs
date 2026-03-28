using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static JanusRequest.HttpClientTree;

namespace JanusRequest.Nodes
{
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

            throw new NotSupportedException($"Member type {member.MemberType} is not supported.");
        }

        public void Map()
        {
            Map(new HashSet<Type>());
        }

        private void Map(HashSet<Type> visited)
        {
            if (!visited.Add(MemberValueType))
                return;

            foreach (var prop in MemberValueType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var node = Add(prop);
                if (!ReflectionUtils.IsNative(node.MemberValueType, false))
                    node.Map(visited);
            }

            foreach (var method in MemberValueType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (IsValidMethod(method))
                    Add(method);
        }

        public abstract IEnumerable<NodeValue> GetAllValues(object owner, INodeNamer namer);

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
    }
}
