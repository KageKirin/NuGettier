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
    public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageVersions(
        string packageName,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        PackageMetadataResource resource = await Repository.GetResourceAsync<PackageMetadataResource>(
            cancellationToken
        );
        return await resource.GetMetadataAsync(
            packageName,
            includePrerelease: preRelease,
            includeUnlisted: false,
            Cache,
            NullLogger.Instance,
            cancellationToken
        );
    }
}
