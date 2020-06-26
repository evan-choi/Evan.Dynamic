using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Evan.Dynamic.Attributes;
using Evan.Dynamic.Extensions;

namespace Evan.Dynamic
{
    public static class DynamicObject
    {
        private static readonly Type _void = typeof(void);

        public static IObjectProxy<T> CreateProxy<T>(T obj) where T : class
        {
            var type = DefineProxyType(typeof(T));
            return (IObjectProxy<T>)Activator.CreateInstance(type.CreateType(), obj);
        }

        private static TypeBuilder DefineProxyType(Type type)
        {
            string typeName = $"{type.Name}$Proxy";
            Type[] interfaces = { typeof(IObjectProxy<>).MakeGenericType(type) };

            var typeBuilder = DynamicModule.DefineType(typeName, TypeAttributes.Public, typeof(object), interfaces);

            // class > T _object
            var objectField = typeBuilder.DefineField("_object", type, FieldAttributes.Private | FieldAttributes.InitOnly);

            // class > IObjectProxy<T>.Object::get_Object
            var getObjectMethod = interfaces[0].GetMethod("get_Object", BindingFlags.Public | BindingFlags.Instance);

            var getObjectImplMethod = typeBuilder.DefineMethod(
                $"IObjectProxy<{type.FullName}>.get_Object",
                MethodAttributes.Private | MethodAttributes.Virtual,
                type,
                Type.EmptyTypes);

            // body > return _object;
            var il = getObjectImplMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(getObjectImplMethod, getObjectMethod);

            // class > .ctor
            var ctor = typeBuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard,
                new[] { type });

            // body > _object = arg0
            il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, objectField);
            il.Emit(OpCodes.Ret);

            var typeInfo = type.GetTypeInfo();
            DefineProxyProperties(typeBuilder, typeInfo, objectField);
            DefineProxyMethods(typeBuilder, typeInfo, objectField);

            return typeBuilder;
        }

        private static void DefineProxyProperties(TypeBuilder typeBuilder, TypeInfo type, FieldBuilder objectField)
        {
            // TODO: implement
        }

        private static void DefineProxyMethods(TypeBuilder typeBuilder, TypeInfo type, FieldBuilder objectField)
        {
            foreach (var method in type.DeclaredMethods)
            {
                if (!method.IsPublic)
                    continue;

                var nameAttribute = method.GetCustomAttribute<ProxyMethodNameAttribute>();

                var proxyMethodName = nameAttribute?.Name ?? method.Name;
                var proxyMethod = typeBuilder.DefineMethod(proxyMethodName, MethodAttributes.Public);

                if (method.IsGenericMethod)
                {
                    Type[] genericTypes = method.GetGenericArguments();
                    string[] genericNames = genericTypes.Select(t => t.Name).ToArray();
                    GenericTypeParameterBuilder[] genericParameters = proxyMethod.DefineGenericParameters(genericNames);

                    for (int i = 0; i < genericNames.Length; i++)
                    {
                        var genericParameter = genericParameters[i];
                        Type[] constraints = genericTypes[i].GetGenericParameterConstraints();

                        if (constraints.Length > 0)
                        {
                            genericParameter.SetInterfaceConstraints(constraints);
                        }
                    }
                }

                Type[] parameters = method.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                proxyMethod.SetParameters(parameters);
                proxyMethod.SetReturnType(method.ReturnType);

                // body: return _object.METHOD(args..)
                var il = proxyMethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, objectField);

                for (int i = 0; i < parameters.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);

                il.EmitCall(method);

                if (method.ReturnType != _void)
                {
                    var retValue = il.DeclareLocal(method.ReturnType);
                    il.Emit(OpCodes.Stloc, retValue);
                    il.Emit(OpCodes.Ldloc, retValue);
                }

                il.Emit(OpCodes.Ret);
            }
        }
    }
}
