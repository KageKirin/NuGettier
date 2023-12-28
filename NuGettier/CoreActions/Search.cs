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
using Xunit;

namespace NuGettier;

public static partial class Program
{
    private static Command SearchCommand =>
        new Command("search", "search for term or package id")
        {
            SearchTermArgument,
            OutputJsonOption,
            ShortOutputOption,
            SourceRepositoriesOption,
        }.WithHandler(CommandHandler.Create(Search));

    private static async Task<int> Search(
        string searchTerm,
        bool json,
        bool @short,
        IEnumerable<Uri> sources,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        Assert.NotNull(Configuration);
        using var context = new Core.Context(configuration: Configuration!, sources: sources, console: console);
        var results = await context.SearchPackages(searchTerm: searchTerm, cancellationToken: cancellationToken);

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

        return 0;
    }
}
