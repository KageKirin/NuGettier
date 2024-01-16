using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

public static class ILoggerExtension
{
    public static ILogger TraceLocation(
        this ILogger logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogTrace("{0}:{1}", sourceFilePath, sourceLineNumber);
        return logger;
    }

    public static ILogger TraceSource(
        this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogTrace("{0}() {1}:{2}", memberName, sourceFilePath, sourceLineNumber);
        return logger;
    }

    public static ILogger TraceSource(
        this ILogger logger,
        object obj,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogTrace("{0}.{1}() {2}:{3}", obj.GetType().FullName, memberName, sourceFilePath, sourceLineNumber);
        return logger;
    }

        public static ILogger DebugLocation(
        this ILogger logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogDebug("{0}:{1}", sourceFilePath, sourceLineNumber);
        return logger;
    }
}
