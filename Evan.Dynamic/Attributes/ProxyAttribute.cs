using System;

namespace Evan.Dynamic.Attributes
{
    public abstract class ProxyAttribute : Attribute
    {
        public string Name { get; }

        protected ProxyAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
