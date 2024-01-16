using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
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

namespace NuGettier.Core;

public partial class Context
{
    public virtual async Task<IEnumerable<IPackageSearchMetadata>> GetPackageVersions(
        string packageId,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        IEnumerable<PackageMetadataResource> resources = await Repositories.GetResourceAsync<PackageMetadataResource>(
            cancellationToken
        );
        using var nugetLogger = NuGetLogger.Create(LoggerFactory);
        var packages = await resources.GetMetadataAsync(
            packageId,
            includePrerelease: preRelease,
            includeUnlisted: false,
            Cache,
            nugetLogger,
            cancellationToken
        );
        return packages.OrderByDescending(p => p.Identity.Version);
    }
}
