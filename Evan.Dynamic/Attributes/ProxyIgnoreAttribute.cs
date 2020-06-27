﻿using System;

namespace Evan.Dynamic.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class ProxyIgnoreAttribute : ProxyAttribute
    {
    }
}
