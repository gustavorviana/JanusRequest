using System;
using System.Reflection;

namespace JanusRequest.Nodes
{
    internal class NodeValue
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
}
