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

#nullable enable

namespace NuGettier;

public static partial class Program
{
    private static IConfigurationRoot? Configuration;
    static async Task<int> Main(string[] args)
    {
        var Configuration = new ConfigurationBuilder()
            .AddDotNetConfig()
            .Build();

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
