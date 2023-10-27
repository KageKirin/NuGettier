using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Xunit;

#nullable enable

namespace NuGettier;

public static partial class Program
{
    private static IConfigurationRoot? configurationRoot = null;
    private static IConfigurationRoot Configuration
    {
        get
        {
            // Config.GlobalLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", "global.netconfig");
            // Config.SystemLocation = Path.Combine(Directory.GetCurrentDirectory(), "Content", ".netconfig");
            configurationRoot ??= new ConfigurationBuilder()
                .AddDotNetConfig()
                .Build();
            return configurationRoot;
        }
    }

    static async Task<int> Main(string[] args)
    {
        Assert.NotNull(Configuration);


        var cmd = new RootCommand("Extended NuGet helper utility")
        { //
            SearchCommand,
            ListCommand,
            InfoCommand,
            GetCommand,
            UpmCommand,
        };
        cmd.Name = "dotnet-nugettier";

        return await cmd.InvokeAsync(args);
    }
}
