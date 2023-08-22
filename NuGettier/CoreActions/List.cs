using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
            SourceRepositoryOption,
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
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3($"{source.ToString()}");

        FindPackageByIdResource resource =
            await repository.GetResourceAsync<FindPackageByIdResource>();
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            packageName,
            cache,
            NullLogger.Instance,
            cancellationToken
        );

        foreach (NuGetVersion version in versions)
        {
            Console.WriteLine($"* {version}");
        }

        return 0;
    }
}
