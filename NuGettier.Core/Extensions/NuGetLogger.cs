using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;

namespace NuGettier.Core;

class NuGetLogger : NuGet.Common.ILogger, IDisposable
{
    private readonly Microsoft.Extensions.Logging.ILogger Logger;

    public static NuGetLogger Create(Microsoft.Extensions.Logging.ILogger logger) => new NuGetLogger(logger);

    public static NuGetLogger Create(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory) =>
        new NuGetLogger(loggerFactory);

    private NuGetLogger(Microsoft.Extensions.Logging.ILogger logger)
    {
        Logger = logger;
    }

    private NuGetLogger(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<NuGetLogger>();
    }

    public NuGetLogger(NuGetLogger other)
    {
        Logger = other.Logger;
    }

    #region IDisposable
    public virtual void Dispose() { }
    #endregion

    #region NuGet.Common.ILogger
    public virtual void LogDebug(string data) => Logger.LogDebug(data);

    public virtual void LogVerbose(string data) => Logger.LogInformation(data);

    public virtual void LogInformation(string data) => Logger.LogInformation(data);

    public virtual void LogMinimal(string data) => Logger.LogInformation(data);

    public virtual void LogWarning(string data) => Logger.LogWarning(data);

    public virtual void LogError(string data) => Logger.LogError(data);

    public virtual void LogInformationSummary(string data) => Logger.LogInformation(data);

    public virtual void Log(NuGet.Common.LogLevel level, string data)
    {
        switch (level)
        {
            case NuGet.Common.LogLevel.Debug:
                Logger.LogDebug(data);
                break;
            case NuGet.Common.LogLevel.Verbose:
                Logger.LogInformation(data);
                break;
            case NuGet.Common.LogLevel.Information:
                Logger.LogInformation(data);
                break;
            case NuGet.Common.LogLevel.Minimal:
                Logger.LogInformation(data);
                break;
            case NuGet.Common.LogLevel.Warning:
                Logger.LogWarning(data);
                break;
            case NuGet.Common.LogLevel.Error:
                Logger.LogError(data);
                break;
        }
        ;
    }

    public virtual async Task LogAsync(NuGet.Common.LogLevel level, string data) => await Task.Run(() => Log(level, data));

    public virtual void Log(ILogMessage message)
    {
        Log(message.Level, $"{message.Time} [NuGet {message.Code}]: {message.Message}");
    }

    public virtual async Task LogAsync(ILogMessage message)
    {
        await LogAsync(message.Level, $"{message.Time} [NuGet {message.Code}]: {message.Message}");
    }
    #endregion
}
