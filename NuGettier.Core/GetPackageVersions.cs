using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageVersions(
        string packageName,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<PackageMetadataResource> resources = await Repositories.GetResourceAsync<PackageMetadataResource>(
            cancellationToken
        );
        var packages = await resources.GetMetadataAsync(
            packageName,
            includePrerelease: preRelease,
            includeUnlisted: false,
            Cache,
            NullLogger.Instance,
            cancellationToken
        );
        return packages.OrderByDescending(p => p.Identity.Version);
    }
}
