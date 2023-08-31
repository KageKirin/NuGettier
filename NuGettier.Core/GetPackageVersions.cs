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
    public async Task<int> GetPackageVersions(
        string packageName,
        bool preRelease,
        bool json,
        CancellationToken cancellationToken
    )
    {
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

        if (json)
        {
            Console.WriteLine(@$"{JsonSerializer.Serialize(packages)}");
        }
        else
        {
            foreach (IPackageSearchMetadata package in packages)
            {
                Console.WriteLine($"* {package.Identity.Version}");
            }
        }

        return 0;
    }
}
