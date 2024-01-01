using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
    public virtual async Task<IEnumerable<IPackageSearchMetadata>> GetPackageVersions(
        string packageId,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        IEnumerable<PackageMetadataResource> resources = await Repositories.GetResourceAsync<PackageMetadataResource>(
            cancellationToken
        );
        var packages = await resources.GetMetadataAsync(
            packageId,
            includePrerelease: preRelease,
            includeUnlisted: false,
            Cache,
            NullLogger.Instance,
            cancellationToken
        );
        return packages.OrderByDescending(p => p.Identity.Version);
    }
}
