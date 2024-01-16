using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Xunit;

#nullable enable

namespace NuGettier;

public partial class Program
{
    private static IConfigurationRoot? configurationRoot = null;
    private static IConfigurationRoot Configuration
    {
        get
        {
            configurationRoot ??= new ConfigurationBuilder() //
                .AddJsonFile("appconfig.json", optional: false, reloadOnChange: false)
                .AddJsonFile(Path.Join(Environment.CurrentDirectory, "appconfig.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddDotNetConfig()
                .Build();
            return configurationRoot;
        }
    }

    private static ILoggerFactory? mainLoggerFactory = null;
    public static ILoggerFactory MainLoggerFactory
    {
        get
        {
            mainLoggerFactory ??= LoggerFactory.Create(builder =>
            {
                builder
                    .AddConfiguration(Configuration.GetSection("logging"))
                    .AddConsole() //< add console as logging target
                    .AddDebug() //< add debug output as logging target
                    .AddFile() //< add file output as logging target
#if DEBUG
                    .SetMinimumLevel(
                        LogLevel.Trace
                    ) //< set minimum level to trace in Debug
#else
                    .SetMinimumLevel(
                        LogLevel.Error
                    ) //< set minimum level to error in Release
#endif
                ;
            });
            return mainLoggerFactory;
        }
    }

    private static readonly ILogger Logger = MainLoggerFactory.CreateLogger<Program>();

    static async Task<int> Main(string[] args)
    {
        Logger.LogDebug($"called with args: {string.Join(" ", args.Select(a => $"'{a}'"))}");
        Assert.NotNull(Configuration);

        var cmd = new RootCommand("Extended NuGet helper utility")
        { //
            SearchCommand,
            ListCommand,
            InfoCommand,
            GetCommand,
            ListDependenciesCommand,
            UpmCommand,
        };
        cmd.Name = "dotnet-nugettier";

        return await cmd.InvokeAsync(args);
    }
}
