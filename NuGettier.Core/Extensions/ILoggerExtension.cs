using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

    public static ILogger DebugSource(
        this ILogger logger,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogDebug("{0}() {1}:{2}", memberName, sourceFilePath, sourceLineNumber);
        return logger;
    }

    public static ILogger DebugSource(
        this ILogger logger,
        object obj,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        logger.LogDebug("{0}.{1}() {2}:{3}", obj.GetType().FullName, memberName, sourceFilePath, sourceLineNumber);
        return logger;
    }

    public static async Task LogAsync(
        this ILogger logger,
        LogLevel logLevel,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    )
    {
        await Task.Run(() => logger.Log(logLevel, message, args));
        await Task.Delay(1_000, cancellationToken);
    }

    public static async Task LogTraceAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Trace, cancellationToken, message, args);

    public static async Task LogDebugAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Debug, cancellationToken, message, args);

    public static async Task LogInformationAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Information, cancellationToken, message, args);

    public static async Task LogWarningAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Warning, cancellationToken, message, args);

    public static async Task LogErrorAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Error, cancellationToken, message, args);

    public static async Task LogCriticalAsync(
        this ILogger logger,
        CancellationToken cancellationToken,
        string? message,
        params object?[] args
    ) => await logger.LogAsync(LogLevel.Critical, cancellationToken, message, args);
}
