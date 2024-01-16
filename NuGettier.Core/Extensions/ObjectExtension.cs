using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class ObjectExtension
{
    public static string __METHOD__(this object self, [CallerMemberName] string methodName = "") =>
        $"{self.GetType().FullName}.{methodName}()";
}
