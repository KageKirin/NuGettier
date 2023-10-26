using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier;

public static partial class Program
{
    private static Command ListCommand =>
        new Command("list", "list available version for a given package")
        {
            PackageNameArgument,
            IncludePrereleaseOption,
            OutputJsonOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(List));

    private static async Task<int> List(
        string packageName,
        bool preRelease,
        bool json,
        Uri source,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var context = new Core.Context(source: source, console: console);
        var results = await context.GetPackageVersions(
            packageName: packageName,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (json)
        {
            Console.WriteLine(@$"{JsonSerializer.Serialize(results)}");
        }
        else
        {
            foreach (IPackageSearchMetadata result in results)
            {
                Console.WriteLine($"* {result.Identity.Id} {result.Identity.Version}");
            }
        }

        return 0;
    }
}
