using System;
using System.Linq;
using System.Reflection;

namespace NuGettier.Core;

#nullable enable

public static class AssemblyExtension
{
    public static T? GetAssemblyAttribute<T>(this Assembly assembly)
        where T : Attribute
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(T), false);
        if (attributes == null || attributes.Length == 0)
            return null;
        return attributes.OfType<T>().SingleOrDefault();
    }
}
