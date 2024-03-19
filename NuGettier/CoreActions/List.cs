using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Xunit;

namespace NuGettier;

public partial class NuGettierService
{
    private Command ListCommand =>
        new Command("list", "list available version for a given package")
        {
            PackageIdArgument,
            IncludePrereleaseOption,
            OutputJsonOption,
            ShortOutputOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(List));

    private async Task<int> List(
        string packageId,
        bool preRelease,
        bool json,
        bool @short,
        IEnumerable<Uri> sources,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(List));
        Assert.NotNull(Configuration);
        using var context = new Core.Context(
            configuration: Configuration!,
            sources: sources,
            console: console,
            loggerFactory: MainLoggerFactory
        );
        var results = await context.GetPackageVersions(
            packageId: packageId,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (results is null)
        {
            Logger.LogError("failed to get package versions for {0}", packageId);
            return 1;
        }

        if (@short)
        {
            var versions = results.Select(r => r.Identity.Version.ToNormalizedString());
            if (json)
            {
                Console.WriteLine(@$"{JsonSerializer.Serialize(versions, JsonOptions)}");
            }
            else
            {
                foreach (var version in versions)
                {
                    Console.WriteLine($"{version}");
                }
            }
            Logger.LogTrace("exit {0} without error (short mode)", nameof(List));
            return 0;
        }

        if (json)
        {
            Console.WriteLine(@$"{JsonSerializer.Serialize(results, JsonOptions)}");
        }
        else
        {
            foreach (IPackageSearchMetadata result in results)
            {
                Console.WriteLine($"* {result.Identity.Id} {result.Identity.Version}");
            }
        }

        Logger.LogTrace("exit {0} without error", nameof(List));
        return 0;
    }
}
