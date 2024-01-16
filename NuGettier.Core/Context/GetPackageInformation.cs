using System;
using System.Collections.Generic;
using System.CommandLine;
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

namespace NuGettier.Core;

#nullable enable

public partial class Context
{
    public virtual async Task<IPackageSearchMetadata?> GetPackageInformation(
        string packageIdVersion,
        bool preRelease,
        CancellationToken cancellationToken
    )
    {
        using var scope = Logger.TraceLocation().BeginScope(this.__METHOD__());
        packageIdVersion.SplitPackageIdVersion(out var packageId, out var version, out var latest);
        var packages = await GetPackageVersions(
            packageId: packageId,
            preRelease: preRelease,
            cancellationToken: cancellationToken
        );

        if (latest || string.IsNullOrEmpty(version))
        {
            return packages.FirstOrDefault();
        }

        NuGetVersion cmpVersion = new(version);
        return packages.Where(p => p.Identity.Version == cmpVersion).FirstOrDefault();
    }
}
