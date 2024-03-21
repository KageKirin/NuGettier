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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using NuGettier.Upm;
using Xunit;
using ZLogger;

#nullable enable

namespace NuGettier;

public static class Program
{
    public static string[]? CommandLineArgs = null;

    private static IHost? Host = null;

    static async Task<int> Main(string[] args)
    {
        CommandLineArgs = args;

        Host = Microsoft
            .Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(
                (context, builder) =>
                {
                    builder
                        .AddJsonFile("appconfig.json", optional: false, reloadOnChange: false)
                        .AddJsonFile(
                            Path.Join(Environment.CurrentDirectory, "appconfig.json"),
                            optional: true,
                            reloadOnChange: false
                        )
                        .AddEnvironmentVariables("NUGETTIER_")
                        .AddDotNetConfig();
                }
            )
            .ConfigureLogging(
                (context, builder) =>
                {
                    builder
                        .AddConfiguration(context.Configuration.GetSection("logging"))
                        .AddConsole() //< add console as logging target
                        .AddDebug() //< add debug output as logging target
                        .AddZLoggerFile(Path.Join(Environment.CurrentDirectory, "nugettier.log")) //< add file output as logging target
                        .SetMinimumLevel(
#if DEBUG
                            LogLevel.Trace //< set minimum level to trace in Debug
#else
                            LogLevel.Error //< set minimum level to error in Release
#endif
                        );
                }
            )
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddOptions();
                    services.AddHostedService<NuGettierService>();
                    services.AddScoped<INpmrcFactory, NpmrcFactory>();
                    services.AddScoped<IReadmeFactory, ReadmeFactory>();
                    services.AddScoped<ILicenseFactory, LicenseFactory>();
                    services.AddScoped<IChangelogFactory, ChangelogFactory>();
                    services.AddScoped<IMetaFactory, MetaFactory>();
                    services.AddScoped<IGuidFactory, Sha1GuidFactory>();
                    services.AddScoped<IGuidFactory, Md5GuidFactory>();
                    services.AddScoped<IGuidFactory, XxHash128GuidFactory>();
                    services.AddScoped<IGuidFactory, XxHash3GuidFactory>();
                }
            )
            .Build();
        await Host.RunAsync();

        return Environment.ExitCode;
    }
}
