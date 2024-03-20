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
    private Command SearchCommand =>
        new Command("search", "search for term or package id")
        {
            SearchTermArgument,
            OutputJsonOption,
            ShortOutputOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(Search));

    private async Task<int> Search(
        string searchTerm,
        bool json,
        bool @short,
        IEnumerable<Uri> sources,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(nameof(Search));
        Assert.NotNull(Configuration);
        using var context = new Core.Context(
            host: Host,
            configuration: Configuration,
            loggerFactory: MainLoggerFactory,
            logger: MainLoggerFactory.CreateLogger<Core.Context>(),
            console: console,
            sources: sources
        );
        var results = await context.SearchPackages(searchTerm: searchTerm, cancellationToken: cancellationToken);

        if (results is null)
        {
            Logger.LogError("failed to find any packages for {0}", searchTerm);
            return 1;
        }

        if (@short)
        {
            var res = results.ToDictionary(r => r.Identity.Id, r => r.Identity.Version.ToNormalizedString());
            if (json)
            {
                Console.WriteLine(@$"{JsonSerializer.Serialize(res, JsonOptions)}");
            }
            else
            {
                foreach (var kvp in res)
                {
                    Console.WriteLine($"{kvp.Key}@{kvp.Value}");
                }
            }
            Logger.LogTrace("exit {0} without error (short mode)", nameof(Search));
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

        Logger.LogTrace("exit {0} without error", nameof(Search));
        return 0;
    }
}
