using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Xunit;
using ZLogger;

#nullable enable

namespace NuGettier;

/// <summary>
/// NuGettierService is the core of the current implementation
/// and replaces what Program did
/// </summary>
public partial class NuGettierService : IHostedService, IDisposable
{
    private readonly IHost Host;
    private readonly IHostApplicationLifetime HostLifetime;
    private readonly IConfigurationRoot Configuration;
    private readonly ILoggerFactory MainLoggerFactory;
    private readonly ILogger Logger;

    public NuGettierService(
        IHost host,
        IHostApplicationLifetime appLifetime,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        ILogger<NuGettierService> logger
    )
    {
        Host = host;
        HostLifetime = appLifetime;
        Configuration = (IConfigurationRoot)configuration;
        MainLoggerFactory = loggerFactory;
        Logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        HostLifetime.ApplicationStarted.Register(
            () =>
                Task.Run(async () =>
                {
                    try
                    {
                        Environment.ExitCode = await RunAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Unhandled exception!");
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        HostLifetime.StopApplication();
                    }
                })
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose() { }

    async Task<int> RunAsync()
    {
        var cmd = new RootCommand("Extended NuGet helper utility")
        {
            //OutputLogLevelOption,
            SearchCommand,
            ListCommand,
            InfoCommand,
            GetCommand,
            ListDependenciesCommand,
            UpmCommand,
        };
        cmd.Name = "dotnet-nugettier";

        return await cmd.InvokeAsync(Program.CommandLineArgs ?? ["--help"]);
    }
}
