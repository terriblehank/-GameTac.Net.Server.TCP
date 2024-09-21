using System;
using System.Linq;
using System.Reflection;

namespace GameTac.Net.Server.Core.Sys
{
    public class Reflection
    {
        public static Type[] GetChilds<T>()
        {
            var baseType = typeof(T);
            var assembly = Assembly.GetAssembly(baseType);
            if (assembly is null) return [];

            Type[] types = assembly.GetTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToArray();

            return types;
        }
        public static Type[] GetImplements(Type i)
        {
            var assembly = Assembly.GetAssembly(i);
            if (assembly is null) return [];

            Type[] types = assembly.GetTypes().Where(type => type != i && i.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToArray();

            return types;
        }

    }
}