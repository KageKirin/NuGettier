using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace NuGettier;

public static partial class Program
{
    static async Task<int> Main(string[] args)
    {
        var cmd = new RootCommand
        { //
            SearchCommand,
            ListCommand,
            InfoCommand,
            GetCommand,
        };
        cmd.Name = "dotnet-nugettier";
        cmd.Description = "Extended NuGet helper util";

        return await cmd.InvokeAsync(args);
    }
}
