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
    private static Command SearchCommand =>
        new Command("search", "search for term or package name")
        {
            SearchTermArgument,
            OutputJsonOption,
            SourceRepositoryOption,
        }.WithHandler(CommandHandler.Create(Search));

    private static async Task<int> Search(
        string searchTerm,
        bool json,
        Uri source,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3($"{source.ToString()}");

        PackageSearchResource resource = await repository.GetResourceAsync<PackageSearchResource>();
        SearchFilter searchFilter = new SearchFilter(includePrerelease: true);

        IEnumerable<IPackageSearchMetadata> results = await resource.SearchAsync(
            searchTerm,
            searchFilter,
            skip: 0,
            take: 100,
            NullLogger.Instance,
            cancellationToken
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
