using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.CommandLine;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGettier.Core;

public partial class Context
{
    public async Task<int> GetPackageInformation(
        string packageName,
        bool preRelease,
        bool latest,
        string version,
        bool json,
        Uri source,
        IConsole console,
        CancellationToken cancellationToken
    )
    {
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3($"{source.ToString()}");

        PackageMetadataResource resource =
            await repository.GetResourceAsync<PackageMetadataResource>();
        IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
            packageName,
            includePrerelease: preRelease,
            includeUnlisted: false,
            cache,
            NullLogger.Instance,
            cancellationToken
        );

        IPackageSearchMetadata? package = null;
        if (latest)
        {
            package = packages.Last();
        }
        else
        {
            NuGetVersion cmpVersion = new(version);
            package = packages.Where(p => p.Identity.Version == cmpVersion).FirstOrDefault();
        }

        if (package != null)
        {
            if (json)
            {
                Console.WriteLine($"{JsonSerializer.Serialize(package)}");
            }
            else
            {
                Console.WriteLine($"Version: {package.Identity.Version}");
                Console.WriteLine($"Listed: {package.IsListed}");
                Console.WriteLine($"Tags: {package.Tags}");
                Console.WriteLine($"Description: {package.Description}");
                Console.WriteLine($"Authors: {package.Authors}");
            }
        }

        return 0;
    }
}
