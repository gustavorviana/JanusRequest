using System.Reflection;

namespace JanusRequest.Nodes
{
    internal interface INodeNamer
    {
        bool CanEnter(PropertyInfo property);
        void OnEnter(PropertyInfo property);
        void OnLeave();

        string GetName(MemberInfo member);
        bool CanMap(MemberInfo member);

        string GetName();
        string GetFullName(string name);
    }
}
