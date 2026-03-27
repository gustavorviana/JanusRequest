using System.Collections.Generic;
using System.Reflection;

namespace JanusRequest.Nodes
{
    internal class NodeNamer : INodeNamer
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
}
