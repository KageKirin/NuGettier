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
}
