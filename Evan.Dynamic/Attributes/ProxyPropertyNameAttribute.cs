using System;

namespace Evan.Dynamic.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ProxyPropertyNameAttribute : ProxyAttribute
    {
        public ProxyPropertyNameAttribute(string name) : base(name)
        {
        }
    }
}
