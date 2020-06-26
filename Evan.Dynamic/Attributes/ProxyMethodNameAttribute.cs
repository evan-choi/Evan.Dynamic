using System;

namespace Evan.Dynamic.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProxyMethodNameAttribute : ProxyAttribute
    {
        public ProxyMethodNameAttribute(string name) : base(name)
        {
        }
    }
}
