using System;

namespace Evan.Dynamic.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class ProxyNameAttribute : ProxyAttribute
    {
        public string Name { get; }

        public ProxyNameAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
