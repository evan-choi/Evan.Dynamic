using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Evan.Dynamic
{
    internal static class DynamicModule
    {
        private static readonly ModuleBuilder _module;

        static DynamicModule()
        {
            var assemblyName = new AssemblyName("Evan.Dynamic");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            _module = assembly.DefineDynamicModule("DynamicModule");
        }

        #region DefineType
        public static TypeBuilder DefineType(string name)
        {
            return _module.DefineType(name);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr)
        {
            return _module.DefineType(name, attr);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {
            return _module.DefineType(name, attr, parent);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, int typeSize)
        {
            return _module.DefineType(name, attr, parent, typeSize);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize, int typeSize)
        {
            return _module.DefineType(name, attr, parent, packingSize, typeSize);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            return _module.DefineType(name, attr, parent, interfaces);
        }

        public static TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, PackingSize packingSize)
        {
            return _module.DefineType(name, attr, parent, packingSize);
        }
        #endregion
    }
}
