using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Evan.Dynamic.Attributes;
using Evan.Dynamic.Extensions;

namespace Evan.Dynamic
{
    public static class DynamicObject
    {
        private const BindingFlags allInstances = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static IObjectProxy<T> CreateProxy<T>(T obj) where T : class
        {
            var type = DefineProxyType(typeof(T));
            return (IObjectProxy<T>)Activator.CreateInstance(type.CreateType(), obj);
        }

        private static TypeBuilder DefineProxyType(Type type)
        {
            string typeName = $"{type.Name}$Proxy";
            var objectProxyInterfaceType = typeof(IObjectProxy<>).MakeGenericType(type);

            var typeBuilder = DynamicModule.DefineType(typeName, TypeAttributes.Public, typeof(object));

            // class Type$Proxy : IObjectProxy<Type>, interfaces..

            typeBuilder.AddInterfaceImplementation(objectProxyInterfaceType);

            foreach (var interfaceType in type.GetInterfaces())
            {
                typeBuilder.AddInterfaceImplementation(interfaceType);
            }

            // class > T _object
            var objectField = typeBuilder.DefineField("_object", type, FieldAttributes.Private | FieldAttributes.InitOnly);

            // class > IObjectProxy<T>.Object::get_Object
            var getObjectMethod = objectProxyInterfaceType.GetMethod("get_Object", BindingFlags.Public | BindingFlags.Instance);

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
            MethodInfo[] declaredMethods = type.GetMethods(allInstances).ToArray();
            IList<MethodInfo>[] interfaceMaps = null;

            foreach (var interfaceMapping in type.GetInterfaces().Select(type.GetInterfaceMap))
            {
                for (int i = 0; i < interfaceMapping.TargetMethods.Length; i++)
                {
                    interfaceMaps ??= new IList<MethodInfo>[declaredMethods.Length];

                    int index = Array.IndexOf(declaredMethods, interfaceMapping.TargetMethods[i]);
                    IList<MethodInfo> targetInterfaceMethods = interfaceMaps[index];

                    if (targetInterfaceMethods == null)
                    {
                        targetInterfaceMethods = new List<MethodInfo>();
                        interfaceMaps[index] = targetInterfaceMethods;
                    }

                    targetInterfaceMethods.Add(interfaceMapping.InterfaceMethods[i]);
                }
            }

            for (int i = 0; i < declaredMethods.Length; i++)
            {
                var method = declaredMethods[i];
                IList<MethodInfo> interfaceMap = interfaceMaps?[i];

                if (interfaceMap?.Count > 0)
                {
                    foreach (var interfaceMethod in interfaceMap)
                    {
                        DefineProxyMethod(typeBuilder, type, objectField, method, interfaceMethod);
                    }
                }
                else
                {
                    if (!method.IsPublic || method.DeclaringType == typeof(object))
                        continue;

                    DefineProxyMethod(typeBuilder, type, objectField, method, null);
                }
            }
        }

        private static void DefineProxyMethod(TypeBuilder typeBuilder, TypeInfo type, FieldBuilder objectField, MethodInfo declaredMethod, MethodInfo interfaceMethod)
        {
            string proxyMethodName;
            bool implExplicit = interfaceMethod != null && !declaredMethod.IsPublic;

            if (implExplicit)
            {
                proxyMethodName = $"{interfaceMethod.DeclaringType!.FullName}.{interfaceMethod.Name}";
            }
            else
            {
                var nameAttribute = declaredMethod.GetCustomAttribute<ProxyNameAttribute>();
                proxyMethodName = nameAttribute?.Name ?? declaredMethod.Name;
            }

            var proxyMethodAttributes = implExplicit ?
                MethodAttributes.Private | MethodAttributes.Virtual :
                declaredMethod.Attributes;

            var proxyMethod = typeBuilder.DefineMethod(proxyMethodName, proxyMethodAttributes);

            if (declaredMethod.IsGenericMethod)
            {
                Type[] genericTypes = declaredMethod.GetGenericArguments();
                string[] genericNames = genericTypes.Select(t => t.Name).ToArray();
                GenericTypeParameterBuilder[] genericParameters = proxyMethod.DefineGenericParameters(genericNames);

                for (int j = 0; j < genericNames.Length; j++)
                {
                    var genericParameter = genericParameters[j];
                    Type[] constraints = genericTypes[j].GetGenericParameterConstraints();

                    if (constraints.Length > 0)
                    {
                        genericParameter.SetInterfaceConstraints(constraints);
                    }
                }
            }

            Type[] parameters = declaredMethod.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            proxyMethod.SetParameters(parameters);
            proxyMethod.SetReturnType(declaredMethod.ReturnType);

            // body: return _object.METHOD(args..)
            var il = proxyMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, objectField);

            for (int j = 0; j < parameters.Length; j++)
                il.Emit(OpCodes.Ldarg, j + 1);

            il.EmitCall(implExplicit ? interfaceMethod : declaredMethod);

            if (declaredMethod.ReturnType != typeof(void))
            {
                var retValue = il.DeclareLocal(declaredMethod.ReturnType);
                il.Emit(OpCodes.Stloc, retValue);
                il.Emit(OpCodes.Ldloc, retValue);
            }

            il.Emit(OpCodes.Ret);

            if (interfaceMethod != null)
            {
                typeBuilder.DefineMethodOverride(proxyMethod, interfaceMethod);
            }
        }
    }
}
