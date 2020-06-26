using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Evan.Dynamic.Extensions
{
    public static class ILGeneratorExtension
    {
        public static void EmitBox(this ILGenerator generator, Type type)
        {
            generator.Emit(OpCodes.Box, type);
        }

        public static void EmitUnbox(this ILGenerator generator, Type type)
        {
            generator.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
        }

        public static void EmitCall(this ILGenerator generator, MethodInfo methodInfo)
        {
            if (methodInfo.DeclaringType != null && methodInfo.DeclaringType.IsValueType)
                generator.Emit(OpCodes.Call, methodInfo);
            else
                generator.Emit(OpCodes.Callvirt, methodInfo);
        }
    }
}
